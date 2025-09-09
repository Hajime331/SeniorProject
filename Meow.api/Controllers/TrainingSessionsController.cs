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


    // PUT /api/TrainingSessions/{id}/complete
    // 完成訓練：寫入 EndedAt 與 CompletedFlag=true（外加可選的 CaloriesBurned/PointsAwarded/Notes），回傳更新後的明細。
    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<TrainingSessionDetailDto>> Complete(Guid id, [FromBody] TrainingSessionCompleteDto dto)
    {
        var session = await _db.TrainingSessions
            .Include(s => s.TrainingSessionItems)
            .FirstOrDefaultAsync(s => s.SessionID == id);

        if (session is null) return NotFound();

        // 1) 決定結束時間（預設用現在 UTC；也可用 dto.EndedAt）
        var ended = (dto?.EndedAt ?? DateTime.UtcNow);
        if (ended < session.StartedAt) ended = session.StartedAt; // 防守：避免倒退

        // 2) 寫入狀態
        session.EndedAt = ended;
        session.CompletedFlag = dto?.CompletedFlag ?? true;
        session.CaloriesBurned = dto?.CaloriesBurned;
        session.PointsAwarded = dto?.PointsAwarded;

        // dto 是 null 就整體回 null，不拋例外
        // string.IsNullOrWhiteSpace(...)：檢查字串是否為 null、空字串、或只有空白
        if (!string.IsNullOrWhiteSpace(dto?.Notes))
        {
            // 告訴編譯器「此處我確定不是 null」
            session.Notes = dto!.Notes; // 若你希望保留舊 notes 就改成追加
        }

        await _db.SaveChangesAsync();

        // 3) 回傳最新明細（沿用你現有的投影邏輯）
        return await GetById(id);
    }


    
    [HttpPut("items")]
    public async Task<ActionResult<TrainingSessionItemDto>> UpdateItem([FromBody] TrainingSessionItemUpdateDto dto)
    {
        if (dto is null || dto.SessionItemID == Guid.Empty)
            return BadRequest("SessionItemID is required.");

        // 找到使用者要更新的那個 SessionItem，而且連同它的 Video 物件一起載入，方便稍後回傳 VideoTitle
        var item = await _db.TrainingSessionItems
            .Include(i => i.Video) // 為了回傳 VideoTitle
            .FirstOrDefaultAsync(i => i.SessionItemID == dto.SessionItemID);

        if (item is null) return NotFound("Session item not found.");

        // —— 欄位逐一「若有值才更新」——
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            // 防呆：只接受 Done/Skipped/Partial（對應你的 CHECK 約束）
            var ok = dto.Status is "Done" or "Skipped" or "Partial";
            if (!ok) return BadRequest("Invalid status.");
            item.Status = dto.Status;
        }
        if (dto.ActualReps.HasValue) item.ActualReps = dto.ActualReps;
        if (dto.ActualWeight.HasValue) item.ActualWeight = dto.ActualWeight;
        if (dto.ActualDurationSec.HasValue) item.ActualDurationSec = dto.ActualDurationSec;
        if (dto.ActualRestSec.HasValue) item.ActualRestSec = dto.ActualRestSec;
        if (dto.RoundsDone.HasValue) item.RoundsDone = dto.RoundsDone;

        // 只有當前端真的有傳 Note 參數時才更新；而且如果傳的是空字串，就把資料庫的 Note 清空（設 null）
        // 跟 !string.IsNullOrWhiteSpace() 不一樣，因為它允許「空字串」進來
        // 如果 dto.Note 是 null、空字串或全空白 → 存回資料庫就改成 null。否則 → 存使用者輸入的內容
        if (dto.Note is not null) item.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note;

        await _db.SaveChangesAsync();

        // 回傳更新後的單筆 DTO（輕量）
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
