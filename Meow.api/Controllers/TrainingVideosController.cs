using Meow.Api.Data;
using Meow.Api.Infrastructure;
using Meow.Shared.Dtos.Videos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TrainingVideosController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrainingVideosController(AppDbContext db) => _db = db;

    // GET /api/TrainingVideos?keyword=&tagIds=guid,guid&status=Published
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingVideoListItemDto>>> Get(
        [FromQuery] string? keyword,
        [FromQuery] string? tagIds,
        [FromQuery] string? status)
    {
        var q = _db.TrainingVideos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(v => v.Title.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(v => v.Status == status);

        // 解析 tagIds -> Guid set
        HashSet<Guid> tagSet = new();
        if (!string.IsNullOrWhiteSpace(tagIds))
        {
            foreach (var s in tagIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (Guid.TryParse(s, out var g)) tagSet.Add(g);
        }
        if (tagSet.Count > 0)
            q = q.Where(v => _db.VideoTagMaps.Any(m => m.VideoID == v.VideoID && tagSet.Contains(m.TagID)));

        var list = await q
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new TrainingVideoListItemDto
            {
                VideoId = v.VideoID,
                Title = v.Title,
                BodyPart = v.BodyPart,
                Url = v.Url,
                DurationSec = v.DurationSec,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                TagIds = _db.VideoTagMaps.Where(m => m.VideoID == v.VideoID).Select(m => m.TagID).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/TrainingVideos/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingVideoDetailDto>> GetById(Guid id)
    {
        var dto = await _db.TrainingVideos.AsNoTracking()
            .Where(v => v.VideoID == id)
            .Select(v => new    TrainingVideoDetailDto
            {
                VideoId = v.VideoID,
                Title = v.Title,
                BodyPart = v.BodyPart,
                Url = v.Url,
                DurationSec = v.DurationSec,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                TagIds = _db.VideoTagMaps.Where(m => m.VideoID == v.VideoID).Select(m => m.TagID).ToList()
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }


    // POST /api/TrainingVideos
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TrainingVideoDetailDto>> Create([FromBody] TrainingVideoCreateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        var validStatus = new[] { "Draft", "Published", "Archived" }; // 你原本就有的白名單
        if (!validStatus.Contains(dto.Status)) return BadRequest("Invalid Status");



        var me = User.GetMemberId(); // ★ 取登入者
        var v = new TrainingVideo
        {
            VideoID = Guid.NewGuid(),
            Title = dto.Title,
            BodyPart = dto.BodyPart,
            Url = dto.Url,
            DurationSec = dto.DurationSec,
            Status = dto.Status,
            CreatedByMemberID = me,             // ★ 寫入擁有者
            ThumbnailUrl = dto.ThumbnailUrl     // ★ 新增縮圖（可為 null）
        };
        _db.TrainingVideos.Add(v);

        foreach (var tid in (dto.TagIds ?? new List<Guid>()).Distinct())
            _db.VideoTagMaps.Add(new VideoTagMap { VideoID = v.VideoID, TagID = tid });

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = v.VideoID }, new TrainingVideoDetailDto
        {
            VideoId = v.VideoID,
            Title = v.Title,
            BodyPart = v.BodyPart,
            Url = v.Url,
            DurationSec = v.DurationSec,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            ThumbnailUrl = v.ThumbnailUrl,  // ★ 帶回縮圖
            TagIds = await _db.VideoTagMaps.Where(m => m.VideoID == v.VideoID).Select(m => m.TagID).ToListAsync()
        });
    }

    // PUT /api/TrainingVideos/{id}/status
    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TrainingVideoStatusDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoID == id);
        if (v is null) return NotFound();

        if (dto is null || string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest("Status required.");

        var me = User.GetMemberId();
        var isAdmin = User.IsAdmin();
        if (!isAdmin && v.CreatedByMemberID != me)
            return Forbid(); // ★ 權限限制

        v.Status = dto.Status;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/TrainingVideos/{id}/tags
    [Authorize]
    [HttpPut("{id:guid}/tags")]
    public async Task<IActionResult> UpdateTags(Guid id, [FromBody] IEnumerable<Guid> tagIds)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoID == id);
        if (v is null) return NotFound();

        var me = User.GetMemberId();
        var isAdmin = User.IsAdmin();
        if (!isAdmin && v.CreatedByMemberID != me)
            return Forbid();

        var old = _db.VideoTagMaps.Where(m => m.VideoID == id);
        _db.VideoTagMaps.RemoveRange(old);

        foreach (var tid in (tagIds ?? Enumerable.Empty<Guid>()).Distinct())
            _db.VideoTagMaps.Add(new VideoTagMap { VideoID = id, TagID = tid });

        await _db.SaveChangesAsync();
        return NoContent();
    }


    // PUT /api/TrainingVideos/{id}
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TrainingVideoUpdateDto dto)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        if (id != dto.VideoId) return BadRequest("ID mismatch.");

        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoID == id);
        if (v == null) return NotFound();

        // 僅影片建立者或管理者可修改
        var me = User.GetMemberId();
        var isAdmin = User.IsAdmin();
        if (!isAdmin && v.CreatedByMemberID != me) return Forbid();

        // 檢查狀態合法
        var validStatus = new[] { "Draft", "Published", "Archived" };
        if (!validStatus.Contains(dto.Status)) return BadRequest("Invalid status.");

        // 更新欄位
        v.Title = dto.Title;
        v.BodyPart = dto.BodyPart;
        v.Url = dto.Url;
        v.DurationSec = dto.DurationSec;
        v.Status = dto.Status;
        v.UpdatedAt = DateTime.UtcNow;

        // 更新標籤：先清除再新增
        var oldTagMaps = _db.VideoTagMaps.Where(m => m.VideoID == id);
        _db.VideoTagMaps.RemoveRange(oldTagMaps);
        foreach (var tid in (dto.TagIds ?? new List<Guid>()).Distinct())
        {
            _db.VideoTagMaps.Add(new VideoTagMap { VideoID = id, TagID = tid });
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/TrainingVideos/{id}
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();

        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoID == id);
        if (v == null) return NotFound();

        // 僅管理者可刪除影片
        if (!User.IsAdmin()) return Forbid();

        // 移除標籤關聯
        var tagMaps = _db.VideoTagMaps.Where(m => m.VideoID == id);
        _db.VideoTagMaps.RemoveRange(tagMaps);

        _db.TrainingVideos.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
