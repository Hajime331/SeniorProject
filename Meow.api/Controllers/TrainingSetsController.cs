using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.TrainingSets;
using Meow.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class TrainingSetsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrainingSetsController(AppDbContext db) => _db = db;

    // GET /api/TrainingSets?keyword=&status=Active
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingSetListItemDto>>> Get([FromQuery] string? keyword, [FromQuery] string? status)
    {
        var q = _db.TrainingSets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(s => s.Name.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == status);

        var list = await q
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new TrainingSetListItemDto(
                s.SetID, s.Name, s.BodyPart, s.Equipment, s.Difficulty, s.EstimatedDurationSec,
                _db.SetTagMaps.Where(m => m.SetID == s.SetID).Select(m => m.TagID).ToList(),
                _db.TrainingSetItems.Count(i => i.SetID == s.SetID)
            ))
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/TrainingSets （建立 + Items + TagIds）
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TrainingSetDetailDto>> Create([FromBody] TrainingSetCreateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required.");

        if (dto.Items is null || dto.Items.Count == 0) return BadRequest("At least 1 item.");

        // ★ 驗證所有 VideoId 存在
        var videoIds = dto.Items.Select(i => i.VideoId).ToHashSet();
        var existingVideoIds = await _db.TrainingVideos
            .Where(v => videoIds.Contains(v.VideoID))
            .Select(v => v.VideoID)
            .ToListAsync();
        if (existingVideoIds.Count != videoIds.Count)
            return BadRequest("Some VideoId not found.");

        // 驗證 TagIds 存在
        if (dto.TagIds is not null && dto.TagIds.Count > 0)
        {
            var tagIds = dto.TagIds.Distinct().ToList();
            var okCount = await _db.Tags.CountAsync(t => tagIds.Contains(t.TagID));
            if (okCount != tagIds.Count) return BadRequest("Some TagId not found.");
        }


        var set = new TrainingSet
        {
            SetID = Guid.NewGuid(),
            Name = dto.Name!,
            BodyPart = dto.BodyPart ?? "全身",
            Equipment = dto.Equipment ?? "無器材",
            Difficulty = dto.Difficulty,
            EstimatedDurationSec = dto.EstimatedDurationSec,
            IsCustom = true, OwnerMemberID = User.GetMemberId(), Status = "Active"
        };

        _db.TrainingSets.Add(set);

        // Items
        var order = 1;
        foreach (var it in dto.Items.OrderBy(x => x.OrderNo ?? int.MaxValue))
        {
            _db.TrainingSetItems.Add(new TrainingSetItem
            {
                SetItemID = Guid.NewGuid(),
                SetID = set.SetID,
                VideoID = it.VideoId,
                OrderNo = it.OrderNo ?? order++,
                TargetReps = it.TargetReps,
                RestSec = it.RestSec,
                Rounds = it.Rounds
            });
        }

        // Tags
        if (dto.TagIds is not null)
        {
            foreach (var tid in dto.TagIds.Distinct())
                _db.SetTagMaps.Add(new SetTagMap { SetID = set.SetID, TagID = tid });
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = set.SetID }, await ProjectDetail(set.SetID));
    }

    // 重構：取得單筆詳細資料的共用方法
    private async Task<TrainingSetDetailDto> ProjectDetail(Guid id)
    {
        return await _db.TrainingSets.AsNoTracking()
            .Where(s => s.SetID == id)
            .Select(s => new TrainingSetDetailDto
            {
                SetID = s.SetID,
                Name = s.Name,
                BodyPart = s.BodyPart,
                Equipment = s.Equipment,
                Difficulty = s.Difficulty,
                EstimatedDurationSec = s.EstimatedDurationSec,
                IsCustom = s.IsCustom,
                OwnerMemberID = s.OwnerMemberID,
                Status = s.Status,
                TagIds = _db.SetTagMaps.Where(m => m.SetID == s.SetID).Select(m => m.TagID).ToList(),
                Items = _db.TrainingSetItems
                    .Where(i => i.SetID == s.SetID)
                    .OrderBy(i => i.OrderNo)
                    .Select(i => new TrainingSetItemDto(i.SetItemID, i.VideoID, i.OrderNo, i.TargetReps, i.RestSec, i.Rounds))
                    .ToList()
            })
            .FirstAsync();
    }


    // GET /api/TrainingSets/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingSetDetailDto>> GetById(Guid id)
    {
        // 直接重用你的 ProjectDetail(id)
        var exists = await _db.TrainingSets.AsNoTracking().AnyAsync(s => s.SetID == id);
        if (!exists) return NotFound();

        var dto = await ProjectDetail(id);
        return Ok(dto);
    }


    // PUT /api/TrainingSets/{id}
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TrainingSetUpdateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        if (id != dto.SetId) return BadRequest("ID mismatch.");

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required.");

        if (dto.Items == null || dto.Items.Count == 0) return BadRequest("At least 1 item.");

        // ★ 驗證所有 VideoId 存在
        var videoIds = dto.Items.Select(i => i.VideoId).ToHashSet();
        var existingVideoIds = await _db.TrainingVideos
            .Where(v => videoIds.Contains(v.VideoID))
            .Select(v => v.VideoID)
            .ToListAsync();
        if (existingVideoIds.Count != videoIds.Count)
            return BadRequest("Some VideoId not found.");

        // 驗證 TagIds 存在
        if (dto.TagIds is not null && dto.TagIds.Count > 0)
        {
            var tagIds = dto.TagIds.Distinct().ToList();
            var okCount = await _db.Tags.CountAsync(t => tagIds.Contains(t.TagID));
            if (okCount != tagIds.Count) return BadRequest("Some TagId not found.");
        }

        // 讀取現有資料（包含 Items）
        var set = await _db.TrainingSets.Include(s => s.TrainingSetItems).FirstOrDefaultAsync(s => s.SetID == id);
        if (set == null) return NotFound();

        // 權限：管理者或擁有者
        var me = User.GetMemberId();
        if (!User.IsAdmin() && set.OwnerMemberID != me) return Forbid();

        // 更新主檔
        set.Name = dto.Name;
        set.BodyPart = dto.BodyPart;
        set.Equipment = dto.Equipment;
        set.Difficulty = dto.Difficulty;
        set.EstimatedDurationSec = dto.EstimatedDurationSec ?? 0;
        set.UpdatedAt = DateTime.UtcNow;

        // 更新 Tags：先移除原有，再新增
        var oldTagMaps = _db.SetTagMaps.Where(m => m.SetID == id);
        _db.SetTagMaps.RemoveRange(oldTagMaps);
        foreach (var tid in (dto.TagIds ?? new List<Guid>()).Distinct())
        {
            _db.SetTagMaps.Add(new SetTagMap { SetID = id, TagID = tid });
        }

        // 更新 Items：先找出現有項目，比對 dto.Items
        var existingItems = await _db.TrainingSetItems.Where(i => i.SetID == id).ToListAsync();
        // 刪除不再存在的項目
        var dtoItemIds = dto.Items.Where(i => i.SetItemId.HasValue).Select(i => i.SetItemId.Value).ToHashSet();
        foreach (var oldItem in existingItems.Where(i => !dtoItemIds.Contains(i.SetItemID)))
        {
            _db.TrainingSetItems.Remove(oldItem);
        }
        // 更新或新增項目
        
        int order = 1;
        foreach (var it in dto.Items.OrderBy(x => x.OrderNo))
        {
            if (it.SetItemId.HasValue)
            {
                var item = existingItems.First(x => x.SetItemID == it.SetItemId.Value);
                item.VideoID = it.VideoId;
                item.OrderNo = it.OrderNo;
                item.TargetReps = it.TargetReps;
                item.RestSec = it.RestSec;
                item.Rounds = it.Rounds;
            }
            else
            {
                _db.TrainingSetItems.Add(new TrainingSetItem
                {
                    SetItemID = Guid.NewGuid(),
                    SetID = id,
                    VideoID = it.VideoId,
                    OrderNo = it.OrderNo,
                    TargetReps = it.TargetReps,
                    RestSec = it.RestSec,
                    Rounds = it.Rounds
                });
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/TrainingSets/{id}
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        var set = await _db.TrainingSets.FirstOrDefaultAsync(s => s.SetID == id);
        if (set == null) return NotFound();

        var me = User.GetMemberId();
        if (!User.IsAdmin() && set.OwnerMemberID != me) return Forbid();

        // 移除關聯項目與標籤
        var items = _db.TrainingSetItems.Where(i => i.SetID == id);
        _db.TrainingSetItems.RemoveRange(items);
        var tagMaps = _db.SetTagMaps.Where(m => m.SetID == id);
        _db.SetTagMaps.RemoveRange(tagMaps);

        _db.TrainingSets.Remove(set);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
