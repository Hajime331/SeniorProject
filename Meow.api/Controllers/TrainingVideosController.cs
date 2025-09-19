using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.Videos;

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
            q = q.Where(v => _db.VideoTagMaps.Any(m => m.VideoId == v.VideoId && tagSet.Contains(m.TagID)));

        var list = await q
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new TrainingVideoListItemDto
            {
                VideoId = v.VideoId,
                Title = v.Title,
                BodyPart = v.BodyPart,
                Url = v.Url,
                DurationSec = v.DurationSec,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                TagIds = _db.VideoTagMaps.Where(m => m.VideoId == v.VideoId).Select(m => m.TagID).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/TrainingVideos/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingVideoDetailDto>> GetById(Guid id)
    {
        var dto = await _db.TrainingVideos.AsNoTracking()
            .Where(v => v.VideoId == id)
            .Select(v => new    TrainingVideoDetailDto
            {
                VideoId = v.VideoId,
                Title = v.Title,
                BodyPart = v.BodyPart,
                Url = v.Url,
                DurationSec = v.DurationSec,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                TagIds = _db.VideoTagMaps.Where(m => m.VideoId == v.VideoId).Select(m => m.TagID).ToList()
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }


    // POST /api/TrainingVideos
    [HttpPost]
    public async Task<ActionResult<TrainingVideoDetailDto>> Create([FromBody] TrainingVideoCreateDto dto)
    {
        // 基本檢查：Title/Url 已靠 DTO 必填；再做白名單
        var validStatus = new[] { "Draft", "Published", "Archived" };
        if (!validStatus.Contains(dto.Status)) return BadRequest("Invalid Status");

        var v = new TrainingVideo
        {
            VideoId = Guid.NewGuid(),
            Title = dto.Title,
            BodyPart = dto.BodyPart,       // 不再 ?? 補值，因為 DTO 已是必填
            Url = dto.Url,
            DurationSec = dto.DurationSec, // 必填 int
            Status = dto.Status,
        };
        _db.TrainingVideos.Add(v);

        foreach (var tid in dto.TagIds.Distinct())
            _db.VideoTagMaps.Add(new VideoTagMap { VideoId = v.VideoId, TagID = tid });

        await _db.SaveChangesAsync();

        // 建議新增 GET /api/TrainingVideos/{id}
        return CreatedAtAction(nameof(GetById), new { id = v.VideoId }, new TrainingVideoDetailDto
        {
            VideoId = v.VideoId,
            Title = v.Title,
            BodyPart = v.BodyPart,
            Url = v.Url,
            DurationSec = v.DurationSec,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            TagIds = await _db.VideoTagMaps.Where(m => m.VideoId == v.VideoId).Select(m => m.TagID).ToListAsync()
        });
    }

    // PUT /api/TrainingVideos/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TrainingVideoStatusDto dto)
    {
        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoId == id);
        if (v is null) return NotFound();

        if (dto is null || string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest("Status required.");

        v.Status = dto.Status;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
