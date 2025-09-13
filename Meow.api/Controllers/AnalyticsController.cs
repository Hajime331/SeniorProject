using Meow.Api.Data;
using Meow.Shared.Dtos.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnalyticsController(AppDbContext db) => _db = db;

    // 小工具：取得本週區間（預設: 以「週一為週首」, UTC）
    private static (DateTime startUtc, DateTime endUtc) GetWeekRangeUtc(DateTime? start)
    {
        // 若有 start -> 用該日為週起；否則：用今天（UTC）回推到本週一 00:00
        var todayUtc = DateTime.UtcNow.Date;
        var baseDate = (start?.Date ?? todayUtc);

        // 週一為 1, 週日為 0（這裡讓週一=0）
        int delta = ((int)baseDate.DayOfWeek + 6) % 7; // Monday=0
        var weekStart = baseDate.AddDays(-delta);
        var weekEnd = weekStart.AddDays(7);
        return (weekStart, weekEnd);
    }

    private static (DateTime startUtc, DateTime endUtc) GetWeekRangeByTaipei(DateTime? startLocalDate)
    {
        // 兼容 Windows / Linux 的時區 ID
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); // Windows
        }
        catch
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); // Linux/macOS
        }

        // 基準日：參數（僅取日期部分）或 台北今天的日期
        DateTime taipeiToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        DateTime baseLocal = (startLocalDate?.Date ?? taipeiToday);

        // 週一為週首：把 DayOfWeek 映射為 Monday=0
        int delta = ((int)baseLocal.DayOfWeek + 6) % 7; // Monday=0, Tuesday=1, ..., Sunday=6
        DateTime weekStartLocal = baseLocal.AddDays(-delta);           // 當地 週一 00:00
        DateTime weekEndLocal = weekStartLocal.AddDays(7);           // 當地 下週一 00:00

        // 轉回 UTC（交給 SQL 篩選）
        DateTime weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(weekStartLocal, tz);
        DateTime weekEndUtc = TimeZoneInfo.ConvertTimeToUtc(weekEndLocal, tz);

        return (weekStartUtc, weekEndUtc);
    }

    // ============ Admin：本週彙總 + Top 活躍會員 ============
    // GET /api/Analytics/admin/weekly?start=2025-09-08&take=5
    [HttpGet("admin/weekly")]
    public async Task<ActionResult<AdminWeeklySummaryDto>> AdminWeekly([FromQuery] DateTime? start, [FromQuery] int take = 5)
    {
        var (startUtc, endUtc) = GetWeekRangeByTaipei(start);
        take = Math.Clamp(take, 1, 20);

        // 只統計「已完成」的場次，避免進行中持續增長
        var q = _db.TrainingSessions.AsNoTracking()
            .Where(s => s.CompletedFlag && s.EndedAt != null
                        && s.StartedAt >= startUtc && s.StartedAt < endUtc);

        // 總分鐘（使用 SQL DateDiffMinute；EF 會翻譯）
        var totalMinutes = await q.SumAsync(s =>
            EF.Functions.DateDiffMinute(s.StartedAt, s.EndedAt!.Value));

        var totalSessions = await q.CountAsync();

        var activeMembers = await q.Select(s => s.MemberID).Distinct().CountAsync();

        // Top 活躍（依分鐘、次要依場次）
        var top = await q
            .GroupBy(s => new { s.MemberID, s.Member.Nickname })
            .Select(g => new ActiveMemberStatDto
            {
                MemberID = g.Key.MemberID,
                Nickname = g.Key.Nickname ?? "(無暱稱)",
                Sessions = g.Count(),
                Minutes = g.Sum(s => EF.Functions.DateDiffMinute(s.StartedAt, s.EndedAt!.Value))
            })
            .OrderByDescending(x => x.Minutes).ThenByDescending(x => x.Sessions)
            .Take(take)
            .ToListAsync();

        return Ok(new AdminWeeklySummaryDto
        {
            WeekStartUtc = startUtc,
            WeekEndUtc = endUtc,
            TotalMinutes = totalMinutes,
            TotalSessions = totalSessions,
            ActiveMembers = activeMembers,
            TopActive = top
        });
    }

    // ============ 個人：本週彙總（每日分佈） ============
    // GET /api/Analytics/weekly?memberId=...&start=2025-09-08
    [HttpGet("weekly")]
    public async Task<ActionResult<MemberWeeklySummaryDto>> MemberWeekly([FromQuery] Guid memberId, [FromQuery] DateTime? start)
    {
        if (memberId == Guid.Empty) return BadRequest("memberId is required.");

        var (startUtc, endUtc) = GetWeekRangeUtc(start);

        var q = _db.TrainingSessions.AsNoTracking()
            .Where(s => s.MemberID == memberId
                        && s.CompletedFlag && s.EndedAt != null
                        && s.StartedAt >= startUtc && s.StartedAt < endUtc);

        var sessions = await q.CountAsync();
        var minutes = await q.SumAsync(s =>
            EF.Functions.DateDiffMinute(s.StartedAt, s.EndedAt!.Value));
        var points = await q.SumAsync(s => s.PointsAwarded ?? 0);

        // 每日分佈（以 UTC 日期切日；若你要台北時區，之後可在資料層做轉換）
        var daily = await q
            .GroupBy(s => new { d = DateOnly.FromDateTime(s.StartedAt) })
            .Select(g => new DailyMinutesPointDto
            {
                Date = g.Key.d,
                Minutes = g.Sum(s => EF.Functions.DateDiffMinute(s.StartedAt, s.EndedAt!.Value))
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(new MemberWeeklySummaryDto
        {
            MemberID = memberId,
            WeekStartUtc = startUtc,
            WeekEndUtc = endUtc,
            Sessions = sessions,
            Minutes = minutes,
            Points = points,
            Daily = daily
        });
    }


    [HttpGet("admin/popular-sets")]
    // [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<PopularTrainingSetDto>>> GetPopularSets(
    [FromQuery] DateTime? start,
    [FromQuery] DateTime? end,
    [FromQuery] int take = 10)
    {
        var utcEnd = end?.ToUniversalTime() ?? DateTime.UtcNow;
        var utcStart = start?.ToUniversalTime() ?? utcEnd.AddDays(-28);

        // 先把 Session 和 TrainingSet join 起來，再 group
        var query =
            from s in _db.TrainingSessions.AsNoTracking()
            join set in _db.TrainingSets.AsNoTracking()
                on s.SetID equals set.SetID
            where s.EndedAt != null
               && s.EndedAt >= utcStart
               && s.EndedAt <= utcEnd
            group new { s, set } by new { s.SetID, set.Name } into g
            orderby g.Count() descending
            select new PopularTrainingSetDto
            {
                SetID = g.Key.SetID,
                SetName = g.Key.Name,
                CompletedCount = g.Count(),
                TotalMinutes = g.Sum(x => EF.Functions.DateDiffMinute(x.s.StartedAt, x.s.EndedAt) ?? 0),
                LastCompletedAtUtc = g.Max(x => x.s.EndedAt)
            };

        var result = await query.Take(Math.Max(1, take)).ToListAsync();
        return Ok(result);
    }

}