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

    // GET /api/TrainingSets?keyword=&status=&difficulty=&tagId=
    // GET /api/TrainingSets?keyword=&status=&difficulty=&tagId=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingSetListItemDto>>> Get(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] string? difficulty,
        [FromQuery] Guid? tagId
    )
    {
        // 基礎查詢
        var q = _db.TrainingSets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(s => s.Name.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == status);

        if (!string.IsNullOrWhiteSpace(difficulty))
            q = q.Where(s => s.Difficulty == difficulty);

        if (tagId.HasValue)
            q = q.Where(s => _db.SetTagMaps.Any(m => m.SetID == s.SetID && m.TagID == tagId.Value));

        // 投影成清單 DTO（沿用你原本的欄位）
        var list = await q
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new TrainingSetListItemDto(
                s.SetID,
                s.Name,
                s.BodyPart,
                s.Equipment,
                s.Difficulty,
                s.EstimatedDurationSec,
                _db.SetTagMaps.Where(m => m.SetID == s.SetID).Select(m => m.TagID).ToList(),
                _db.TrainingSetItems.Count(i => i.SetID == s.SetID)
            ))
            .ToListAsync();

        return Ok(list);
    }



    // GET /api/TrainingSets/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingSetDetailDto>> GetById(Guid id)
    {
        var exists = await _db.TrainingSets.AsNoTracking().AnyAsync(s => s.SetID == id);
        if (!exists) return NotFound();

        var dto = await ProjectDetail(id);
        return Ok(dto);
    }


    // GET /api/TrainingSets/meta
    [HttpGet("meta")]
    public async Task<ActionResult<object>> GetMeta()
    {
        // 從現有資料算出可選難度清單（去空白、去重）
        var difficulties = await _db.TrainingSets
            .AsNoTracking()
            .Select(s => s.Difficulty)
            .Where(d => d != null && d != "")
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return Ok(new { difficulties });
    }



    // POST /api/TrainingSets （建立 + Items + TagIds）
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TrainingSetDetailDto>> Create([FromBody] TrainingSetCreateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required.");
        if (dto.Items is null || dto.Items.Count == 0) return BadRequest("At least 1 item.");

        // 驗證所有 VideoId 存在（避免 EF FK 500）
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

        var me = User.GetMemberId();

        var set = new TrainingSet
        {
            SetID = Guid.NewGuid(),
            Name = dto.Name!,
            BodyPart = dto.BodyPart ?? "全身",
            Equipment = dto.Equipment ?? "無器材",
            Difficulty = dto.Difficulty,
            EstimatedDurationSec = dto.EstimatedDurationSec,
            IsCustom = true,
            OwnerMemberID = me,
            Status = "Active"
        };
        _db.TrainingSets.Add(set);

        // Items
        var orderSeed = 1;
        foreach (var it in dto.Items.OrderBy(x => x.OrderNo ?? int.MaxValue))
        {
            _db.TrainingSetItems.Add(new TrainingSetItem
            {
                SetItemID = Guid.NewGuid(),
                SetID = set.SetID,
                VideoID = it.VideoId,
                OrderNo = it.OrderNo ?? orderSeed++,
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

    // PUT /api/TrainingSets/{id}
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TrainingSetUpdateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();
        if (id == Guid.Empty || id != dto.SetId) return BadRequest("Invalid id.");

        var set = await _db.TrainingSets.FirstOrDefaultAsync(s => s.SetID == id);
        if (set is null) return NotFound();

        // 擁有者或 Admin 才能改
        var me = User.GetMemberId();
        var isOwner = set.OwnerMemberID == me;
        if (!isOwner && !User.IsAdmin()) return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required.");
        if (dto.Items is null || dto.Items.Count == 0) return BadRequest("At least 1 item.");

        // 驗證 Video / Tag 存在
        var videoIds = dto.Items.Select(i => i.VideoId).ToHashSet();
        var existingVideoIds = await _db.TrainingVideos.Where(v => videoIds.Contains(v.VideoID)).Select(v => v.VideoID).ToListAsync();
        if (existingVideoIds.Count != videoIds.Count)
            return BadRequest("Some VideoId not found.");

        if (dto.TagIds is not null && dto.TagIds.Count > 0)
        {
            var tagIds = dto.TagIds.Distinct().ToList();
            var okCount = await _db.Tags.CountAsync(t => tagIds.Contains(t.TagID));
            if (okCount != tagIds.Count) return BadRequest("Some TagId not found.");
        }

        // === 先更新主檔 ===
        set.Name = dto.Name!;
        set.BodyPart = dto.BodyPart ?? "全身";
        set.Equipment = dto.Equipment ?? "無器材";
        set.Difficulty = dto.Difficulty;
        set.EstimatedDurationSec = dto.EstimatedDurationSec;

        // === 差異更新 Items（避免整批刪除引發 FK 500）===
        var existingItems = await _db.TrainingSetItems
            .Where(i => i.SetID == id)
            .ToListAsync();

        // 以 SetItemId 建索引
        var existingMap = existingItems.ToDictionary(x => x.SetItemID, x => x);
        // 收集本次保留的 SetItemID
        var keepIds = new HashSet<Guid>();

        int orderSeed = 1;

        // 先依照傳入排序（若沒有 OrderNo 就以順序種子）
        var incoming = dto.Items
            .OrderBy(x => (int?)x.OrderNo ?? int.MaxValue)
            .ToList();

        foreach (var it in incoming)
        {
            if (it.SetItemId.HasValue && existingMap.TryGetValue(it.SetItemId.Value, out var exist))
            {
                // 更新既有項目
                exist.VideoID = it.VideoId;
                exist.OrderNo = (int?)it.OrderNo ?? orderSeed++;
                exist.TargetReps = it.TargetReps;
                exist.RestSec = it.RestSec;
                exist.Rounds = it.Rounds;

                keepIds.Add(exist.SetItemID);
            }
            else
            {
                // 新增新項目
                var newItem = new TrainingSetItem
                {
                    SetItemID = Guid.NewGuid(),
                    SetID = id,
                    VideoID = it.VideoId,
                    OrderNo = (int?)it.OrderNo ?? orderSeed++,
                    TargetReps = it.TargetReps,
                    RestSec = it.RestSec,
                    Rounds = it.Rounds
                };
                _db.TrainingSetItems.Add(newItem);
                keepIds.Add(newItem.SetItemID);
            }
        }

        // 找出要刪除的舊項目（本次沒被保留者）
        var toDelete = existingItems.Where(x => !keepIds.Contains(x.SetItemID)).ToList();
        if (toDelete.Count > 0)
        {
            var delIds = toDelete.Select(x => x.SetItemID).ToList();

            // ★ 檢查是否被 TrainingSessionItem 參考；若有就禁止刪除，回 400
            var inUseIds = await _db.TrainingSessionItems
                .Where(si => delIds.Contains(si.SetItemID))
                .Select(si => si.SetItemID)
                .Distinct()
                .ToListAsync();

            if (inUseIds.Any())
            {
                // 你也可以列出明確是哪幾個 ID，以利除錯
                return BadRequest("Some set items are referenced by sessions and cannot be removed.");
            }

            _db.TrainingSetItems.RemoveRange(toDelete);
        }

        // === 重建 Tag map（這段通常沒有 FK 問題）===
        var oldMaps = await _db.SetTagMaps.Where(m => m.SetID == id).ToListAsync();
        _db.SetTagMaps.RemoveRange(oldMaps);
        if (dto.TagIds is not null)
        {
            foreach (var tid in dto.TagIds.Distinct())
                _db.SetTagMaps.Add(new SetTagMap { SetID = id, TagID = tid });
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
        if (set is null) return NotFound();

        var me = User.GetMemberId();
        var isOwner = set.OwnerMemberID == me;
        if (!isOwner && !User.IsAdmin()) return Forbid();

        var items = await _db.TrainingSetItems.Where(i => i.SetID == id).ToListAsync();
        var maps = await _db.SetTagMaps.Where(m => m.SetID == id).ToListAsync();
        _db.TrainingSetItems.RemoveRange(items);
        _db.SetTagMaps.RemoveRange(maps);
        _db.TrainingSets.Remove(set);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // 共用：將單筆 Set 投影成 Detail DTO
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


    // POST /api/TrainingSets/{id}/cover
    [Authorize]
    [HttpPost("{id:guid}/cover")]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file.");
        var set = await _db.TrainingSets.FirstOrDefaultAsync(s => s.SetID == id);
        if (set == null) return NotFound();

        // 權限（擁有者或 Admin）
        var me = User.GetMemberId();
        if (set.OwnerMemberID != me && !User.IsAdmin()) return Forbid();

        // 限制檔案型態/大小
        var allowed = new[] { "image/png", "image/jpeg", "image/webp" };
        if (!allowed.Contains(file.ContentType)) return BadRequest("Unsupported image type.");
        if (file.Length > 2 * 1024 * 1024) return BadRequest("Max 2MB.");

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "trainingsets");
        Directory.CreateDirectory(uploads);
        var ext = Path.GetExtension(file.FileName); // .png/.jpg/.webp
        var fileName = $"{id}{ext}";
        var fullPath = Path.Combine(uploads, fileName);

        using (var fs = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(fs);

        var urlBase = $"{Request.Scheme}://{Request.Host}";
        set.CoverUrl = $"{urlBase}/uploads/trainingsets/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(new { coverUrl = set.CoverUrl });
    }

}
