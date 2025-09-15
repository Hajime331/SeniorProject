using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.TrainingVideos;

[ApiController]
[Route("api/[controller]")]
public class TrainingVideosController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrainingVideosController(AppDbContext db) => _db = db;

    // GET /api/TrainingVideos?keyword=&tagIds=guid,guid&status=Published
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingVideoDto>>> Get(
        [FromQuery] string? keyword,
        [FromQuery] string? tagIds,
        [FromQuery] string? status)
    {
        var q = _db.TrainingVideos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(v => v.Title.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(v => v.Status == status);

        // 解析 tagIds
        HashSet<Guid> tagSet = new();
        if (!string.IsNullOrWhiteSpace(tagIds))
        {
            foreach (var s in tagIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (Guid.TryParse(s, out var g)) tagSet.Add(g);
        }
        if (tagSet.Count > 0)
        {
            q = q.Where(v => _db.VideoTagMaps.Any(m => m.VideoID == v.VideoID && tagSet.Contains(m.TagID)));
        }

        var list = await q
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new TrainingVideoDto(
                v.VideoID, v.Title, v.BodyPart, v.Url, v.DurationSec, v.Status,
                _db.VideoTagMaps.Where(m => m.VideoID == v.VideoID).Select(m => m.TagID).ToList()
            ))
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/TrainingVideos
    [HttpPost]
    public async Task<ActionResult<TrainingVideoDto>> Create([FromBody] TrainingVideoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Url))
            return BadRequest("Title/Url required.");

        var v = new TrainingVideo
        {
            VideoID = Guid.NewGuid(),
            Title = dto.Title,
            BodyPart = dto.BodyPart ?? "全身",
            Url = dto.Url,
            DurationSec = dto.DurationSec ?? 0,
            Status = dto.Status ?? "Draft",
        };

        _db.TrainingVideos.Add(v);

        if (dto.TagIds is not null)
        {
            foreach (var tid in dto.TagIds.Distinct())
            {
                _db.VideoTagMaps.Add(new VideoTagMap { VideoID = v.VideoID, TagID = tid });
            }
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = v.VideoID }, new TrainingVideoDto(
            v.VideoID, v.Title, v.BodyPart, v.Url, v.DurationSec, v.Status,
            _db.VideoTagMaps.Where(m => m.VideoID == v.VideoID).Select(m => m.TagID).ToList()
        ));
    }

    // PUT /api/TrainingVideos/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TrainingVideoStatusDto dto)
    {
        var v = await _db.TrainingVideos.FirstOrDefaultAsync(x => x.VideoID == id);
        if (v is null) return NotFound();

        if (dto is null || string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest("Status required.");

        v.Status = dto.Status;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
