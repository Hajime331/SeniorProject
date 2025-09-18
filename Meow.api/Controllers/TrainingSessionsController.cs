using Meow.Api.Data;
using Meow.Shared.Dtos.Common;
using Meow.Shared.Dtos.TrainingSessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TrainingSessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrainingSessionsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/TrainingSessions?memberId=&from=&to=&page=1&pageSize=20&tagIds=胸部,拉伸,GUID,...
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TrainingSessionListItemDto>>> Get(
        [FromQuery] Guid memberId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tagIds = null)
    {
        // 參數檢查
        if (memberId == Guid.Empty) return BadRequest("memberId is required.");
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);

        // 1) 解析 tagIds：支援 Guid 與 中文名稱
        var tokens = (tagIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var guidSet = new HashSet<Guid>();
        var nameTokens = new List<string>();
        foreach (var t in tokens)
        {
            if (Guid.TryParse(t, out var g)) guidSet.Add(g);
            else nameTokens.Add(t);
        }

        if (nameTokens.Count > 0)
        {
            var nameIds = await _db.Tags
                .Where(x => nameTokens.Contains(x.Name))
                .Select(x => x.TagID)
                .ToListAsync();
            foreach (var id in nameIds) guidSet.Add(id);
        }

        var q = _db.TrainingSessions
            .AsNoTracking()
            .Where(s => s.MemberID == memberId);

        if (from.HasValue) q = q.Where(s => s.StartedAt >= from.Value);
        if (to.HasValue) q = q.Where(s => s.StartedAt < to.Value.Date.AddDays(1));

        // 2) Tag 篩選（OR 邏輯）
        if (guidSet.Count > 0)
        {
            q = q.Where(s => _db.SetTagMaps.Any(m => m.SetId == s.SetID && guidSet.Contains(m.TagId)));
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new TrainingSessionListItemDto(
                s.SessionID,
                s.StartedAt,
                s.EndedAt,
                s.CompletedFlag,
                s.Set.Name,
                s.Notes,
                s.CaloriesBurned,
                s.PointsAwarded
            )
            {
                TagNames = _db.SetTagMaps
                    .Where(m => m.SetId == s.SetID)
                    .Select(m => m.Tag.Name)
                    .ToList()
            })
            .ToListAsync();

        return Ok(new PagedResultDto<TrainingSessionListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPost] // POST /api/TrainingSessions?memberId=...
    public async Task<ActionResult<TrainingSessionDetailDto>> Create(
        [FromQuery] Guid memberId,
        [FromBody] TrainingSessionCreateDto dto)
    {
        // 參數基本檢查
        if (memberId == Guid.Empty) return BadRequest("memberId is required.");
        if (dto is null || dto.SetID == Guid.Empty) return BadRequest("SetID is required.");

        // 1) 取出 Set 與其 Items（為了複製成 session items）
        var set = await _db.TrainingSets
            .AsNoTracking()
            .Include(s => s.TrainingSetItems)
            .ThenInclude(i => i.Video)
            .FirstOrDefaultAsync(s => s.SetId == dto.SetID);

        if (set is null) return NotFound("TrainingSet not found.");

        // 2) 建立一筆新的 Session（StartedAt 用 UTC）
        var session = new TrainingSession
        {
            SessionID = Guid.NewGuid(),
            MemberID = memberId,
            SetID = set.SetId,
            StartedAt = DateTime.UtcNow,
            CompletedFlag = false,
            Notes = dto.Notes,
            CaloriesBurned = null,
            PointsAwarded = null
        };

        // 3) 由 SetItem 複製成 SessionItem（先不帶成績）
        foreach (var si in set.TrainingSetItems.OrderBy(x => x.OrderNo))
        {
            session.TrainingSessionItems.Add(new TrainingSessionItem
            {
                SessionItemID = Guid.NewGuid(),
                SessionID = session.SessionID,
                SetItemID = si.SetItemId,
                VideoID = si.VideoId,
                OrderNo = si.OrderNo,
                Status = "Done", // 預設；之後可依 UI 改
                ActualReps = null,
                ActualWeight = null,
                ActualDurationSec = null,
                ActualRestSec = null,
                RoundsDone = null,
                Note = null
            });
        }

        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();

        // 4) 重新查一次（不追蹤）→ 投影成 DetailDto，包含 Items 與 VideoTitle
        var result = await _db.TrainingSessions
            .AsNoTracking()
            .Where(s => s.SessionID == session.SessionID)
            .Select(s => new TrainingSessionDetailDto
            {
                SessionID = s.SessionID,
                MemberID = s.MemberID,
                SetID = s.SetID,
                SetName = s.Set.Name,          // 導覽屬性
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt,
                CompletedFlag = s.CompletedFlag,
                Notes = s.Notes,
                CaloriesBurned = s.CaloriesBurned,
                PointsAwarded = s.PointsAwarded,
                Items = s.TrainingSessionItems
                    .OrderBy(i => i.OrderNo)
                    .Select(i => new TrainingSessionItemDto
                    {
                        SessionItemID = i.SessionItemID,
                        SetItemID = i.SetItemID,
                        VideoID = i.VideoID,
                        OrderNo = i.OrderNo,
                        Status = i.Status,
                        ActualReps = i.ActualReps,
                        ActualWeight = i.ActualWeight,
                        ActualDurationSec = i.ActualDurationSec,
                        ActualRestSec = i.ActualRestSec,
                        RoundsDone = i.RoundsDone,
                        Note = i.Note,
                        VideoTitle = i.Video.Title      // 方便前端顯示
                    })
                    .ToList()
            })
            .FirstAsync();

        // 201 + Location
        return CreatedAtAction(nameof(GetById), new { id = result.SessionID }, result);
    }

    // 供 CreatedAtAction 導航使用：GET /api/TrainingSessions/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingSessionDetailDto>> GetById(Guid id)
    {
        var dto = await _db.TrainingSessions
            .AsNoTracking()
            .Where(s => s.SessionID == id)
            .Select(s => new TrainingSessionDetailDto
            {
                SessionID = s.SessionID,
                MemberID = s.MemberID,
                SetID = s.SetID,
                SetName = s.Set.Name,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt,
                CompletedFlag = s.CompletedFlag,
                Notes = s.Notes,
                CaloriesBurned = s.CaloriesBurned,
                PointsAwarded = s.PointsAwarded,
                Items = s.TrainingSessionItems
                    .OrderBy(i => i.OrderNo)
                    .Select(i => new TrainingSessionItemDto
                    {
                        SessionItemID = i.SessionItemID,
                        SetItemID = i.SetItemID,
                        VideoID = i.VideoID,
                        OrderNo = i.OrderNo,
                        Status = i.Status,
                        ActualReps = i.ActualReps,
                        ActualWeight = i.ActualWeight,
                        ActualDurationSec = i.ActualDurationSec,
                        ActualRestSec = i.ActualRestSec,
                        RoundsDone = i.RoundsDone,
                        Note = i.Note,
                        VideoTitle = i.Video.Title
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // PUT /api/TrainingSessions/{id}/complete
    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<TrainingSessionDetailDto>> Complete(Guid id, [FromBody] TrainingSessionCompleteDto dto)
    {
        var session = await _db.TrainingSessions
            .Include(s => s.TrainingSessionItems)
            .FirstOrDefaultAsync(s => s.SessionID == id);

        if (session is null) return NotFound();

        var ended = (dto?.EndedAt ?? DateTime.UtcNow);
        if (ended < session.StartedAt) ended = session.StartedAt;

        session.EndedAt = ended;
        session.CompletedFlag = dto?.CompletedFlag ?? true;
        session.CaloriesBurned = dto?.CaloriesBurned;
        session.PointsAwarded = dto?.PointsAwarded;

        if (!string.IsNullOrWhiteSpace(dto?.Notes))
        {
            session.Notes = dto!.Notes;
        }

        await _db.SaveChangesAsync();

        return await GetById(id);
    }

    [HttpPut("items")]
    public async Task<ActionResult<TrainingSessionItemDto>> UpdateItem([FromBody] TrainingSessionItemUpdateDto dto)
    {
        if (dto is null || dto.SessionItemID == Guid.Empty)
            return BadRequest("SessionItemID is required.");

        var item = await _db.TrainingSessionItems
            .Include(i => i.Video)
            .FirstOrDefaultAsync(i => i.SessionItemID == dto.SessionItemID);

        if (item is null) return NotFound("Session item not found.");

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var ok = dto.Status is "Done" or "Skipped" or "Partial";
            if (!ok) return BadRequest("Invalid status.");
            item.Status = dto.Status;
        }
        if (dto.ActualReps.HasValue) item.ActualReps = dto.ActualReps;
        if (dto.ActualWeight.HasValue) item.ActualWeight = dto.ActualWeight;
        if (dto.ActualDurationSec.HasValue) item.ActualDurationSec = dto.ActualDurationSec;
        if (dto.ActualRestSec.HasValue) item.ActualRestSec = dto.ActualRestSec;
        if (dto.RoundsDone.HasValue) item.RoundsDone = dto.RoundsDone;

        if (dto.Note is not null) item.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note;

        await _db.SaveChangesAsync();

        var result = new TrainingSessionItemDto
        {
            SessionItemID = item.SessionItemID,
            SetItemID = item.SetItemID,
            VideoID = item.VideoID,
            OrderNo = item.OrderNo,
            Status = item.Status,
            ActualReps = item.ActualReps,
            ActualWeight = item.ActualWeight,
            ActualDurationSec = item.ActualDurationSec,
            ActualRestSec = item.ActualRestSec,
            RoundsDone = item.RoundsDone,
            Note = item.Note,
            VideoTitle = item.Video.Title
        };

        return Ok(result);
    }
}
