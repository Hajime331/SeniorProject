using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.TrainingSets;

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
                s.SetId, s.Name, s.BodyPart, s.Equipment, s.Difficulty, s.EstimatedDurationSec,
                _db.SetTagMaps.Where(m => m.SetId == s.SetId).Select(m => m.TagId).ToList(),
                _db.TrainingSetItems.Count(i => i.SetId == s.SetId)
            ))
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/TrainingSets （建立 + Items + TagIds）
    [HttpPost]
    public async Task<ActionResult<TrainingSetDetailDto>> Create([FromBody] TrainingSetCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required.");
        if (dto.Items is null || dto.Items.Count == 0) return BadRequest("At least 1 item.");

        var set = new TrainingSet
        {
            SetId = Guid.NewGuid(),
            Name = dto.Name!,
            BodyPart = dto.BodyPart ?? "全身",
            Equipment = dto.Equipment ?? "無器材",
            Difficulty = dto.Difficulty,
            EstimatedDurationSec = dto.EstimatedDurationSec,
            IsCustom = true, OwnerMemberId = User.GetMemberId(), Status = "Active"
        };

        _db.TrainingSets.Add(set);

        // Items
        var order = 1;
        foreach (var it in dto.Items.OrderBy(x => x.OrderNo ?? int.MaxValue))
        {
            _db.TrainingSetItems.Add(new TrainingSetItem
            {
                SetItemId = Guid.NewGuid(),
                SetId = set.SetId,
                VideoId = it.VideoId,
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
                _db.SetTagMaps.Add(new SetTagMap { SetId = set.SetId, TagId = tid });
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = set.SetId }, await ProjectDetail(set.SetId));
    }

    private async Task<TrainingSetDetailDto> ProjectDetail(Guid id)
    {
        return await _db.TrainingSets.AsNoTracking()
            .Where(s => s.SetId == id)
            .Select(s => new TrainingSetDetailDto
            {
                SetID = s.SetId,
                Name = s.Name,
                BodyPart = s.BodyPart,
                Equipment = s.Equipment,
                Difficulty = s.Difficulty,
                EstimatedDurationSec = s.EstimatedDurationSec,
                IsCustom = s.IsCustom,
                OwnerMemberID = s.OwnerMemberId,
                Status = s.Status,
                TagIds = _db.SetTagMaps.Where(m => m.SetId == s.SetId).Select(m => m.TagId).ToList(),
                Items = _db.TrainingSetItems
                    .Where(i => i.SetId == s.SetId)
                    .OrderBy(i => i.OrderNo)
                    .Select(i => new TrainingSetItemDto(i.SetItemId, i.VideoId, i.OrderNo, i.TargetReps, i.RestSec, i.Rounds))
                    .ToList()
            })
            .FirstAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingSetDetailDto>> GetById(Guid id)
    {
        // 直接重用你的 ProjectDetail(id)
        var exists = await _db.TrainingSets.AsNoTracking().AnyAsync(s => s.SetId == id);
        if (!exists) return NotFound();

        var dto = await ProjectDetail(id);
        return Ok(dto);
    }
}
