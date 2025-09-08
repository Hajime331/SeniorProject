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

    // GET /api/TrainingSessions?memberId=&from=&to=&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TrainingSessionListItemDto>>> Get(
        [FromQuery] Guid memberId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // 如果沒有帶有效的 memberId（Guid.Empty == 0000...），就直接回 400 Bad Request。
        // 這樣避免撈到整個資料庫的所有 Session。
        if (memberId == Guid.Empty) return BadRequest("memberId is required.");

        // 確保 page 至少是 1，避免使用者傳 0 或負數。
        if (page < 1) page = 1;

        // 限制 pageSize 在 1 到 100 之間，避免一次撈太多或太少。
        // Math.Clamp(x, min, max)：把數值限制在範圍內。
        pageSize = Math.Clamp(pageSize, 1, 100);

        // 建立基礎查詢：從 DbContext 的 TrainingSessions 開始。
        // AsNoTracking()：查詢只讀，不追蹤實體，效能較好。
        // Where(...)：只取該會員的 Session。
        var q = _db.TrainingSessions
            .AsNoTracking()
            .Where(s => s.MemberID == memberId);

        // 如果有 from 參數，就加上篩選條件：開始時間 >= from。
        if (from.HasValue) q = q.Where(s => s.StartedAt >= from.Value);

        // 如果有 to 參數，就加上篩選條件：開始時間 < to+1 天。
        // 用 to.Value.Date.AddDays(1) 是為了讓篩選「含當天」。
        if (to.HasValue) q = q.Where(s => s.StartedAt < to.Value.Date.AddDays(1));

        // CountAsync()：先算總筆數，用於分頁的 TotalCount。
        var total = await q.CountAsync();

        // --------- 以下是取出分頁資料 ---------

        // 寫法 1：直接用導覽屬性 Set 拿到關聯的 TrainingSet.Name。
        // EF 會自動產生 JOIN，不一定需要 Include。
        var items = await q
            .OrderByDescending(s => s.StartedAt)   // 依開始時間由新到舊排序
            .Skip((page - 1) * pageSize)           // 跳過前面 (page-1)*pageSize 筆
            .Take(pageSize)                        // 取出 pageSize 筆
            .Select(s => new TrainingSessionListItemDto(
                s.SessionID,
                s.StartedAt,
                s.EndedAt,
                s.CompletedFlag,
                s.Set.Name,             // 導覽屬性：抓 TrainingSet 的 Name
                s.Notes,
                s.CaloriesBurned,
                s.PointsAwarded
            ))
            .ToListAsync();   // 執行查詢並轉成清單

        // 寫法 2：明確 Include（可留著參考）
        // var items = await q
        //     .Include(s => s.Set)
        //     .OrderByDescending(s => s.StartedAt)
        //     .Skip((page - 1) * pageSize)
        //     .Take(pageSize)
        //     .Select(s => new TrainingSessionListItemDto(
        //         s.SessionID, s.StartedAt, s.EndedAt, s.CompletedFlag,
        //         s.Set.Name, s.Notes, s.CaloriesBurned, s.PointsAwarded
        //     ))
        //     .ToListAsync();

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
            .Include(s => s.TrainingSetItems.OrderBy(i => i.OrderNo))
            .ThenInclude(i => i.Video)
            .FirstOrDefaultAsync(s => s.SetID == dto.SetID);

        if (set is null) return NotFound("TrainingSet not found.");

        // 2) 建立一筆新的 Session（StartedAt 用 UTC）
        var session = new TrainingSession
        {
            SessionID = Guid.NewGuid(),
            MemberID = memberId,
            SetID = set.SetID,
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
                SetItemID = si.SetItemID,
                VideoID = si.VideoID,
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
}
