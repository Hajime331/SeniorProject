using Meow.Api.Data;
using Meow.Shared.Dtos.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

    private static TimeZoneInfo GetTaipeiTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
        }
    }

    private static (DateTime startUtc, DateTime endUtc) GetWeekRangeByTaipei(DateTime? startLocalDate)
    {
        var tz = GetTaipeiTimeZone();

        DateTime taipeiToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        DateTime baseLocal = (startLocalDate?.Date ?? taipeiToday);

        int delta = ((int)baseLocal.DayOfWeek + 6) % 7; // Monday=0, Tuesday=1, ..., Sunday=6
        DateTime weekStartLocal = baseLocal.AddDays(-delta);
        DateTime weekEndLocal = weekStartLocal.AddDays(7);

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

        var (startUtc, endUtc) = GetWeekRangeByTaipei(start);
        var tz = GetTaipeiTimeZone();

        var query = _db.TrainingSessions.AsNoTracking()
            .Where(s => s.MemberID == memberId
                        && s.CompletedFlag && s.EndedAt != null
                        && s.StartedAt >= startUtc && s.StartedAt < endUtc);

        var raw = await query
            .Select(s => new
            {
                s.StartedAt,
                s.EndedAt,
                s.PointsAwarded
            })
            .ToListAsync();

        static DateTime EnsureUtc(DateTime dt)
            => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        var normalized = raw.Select(r => new
        {
            StartUtc = EnsureUtc(r.StartedAt),
            EndUtc = r.EndedAt.HasValue ? EnsureUtc(r.EndedAt.Value) : (DateTime?)null,
            Points = r.PointsAwarded ?? 0
        }).ToList();

        int sessions = normalized.Count;
        int minutes = normalized.Sum(r =>
        {
            var end = r.EndUtc ?? r.StartUtc;
            var diff = end - r.StartUtc;
            return diff.TotalMinutes < 0 ? 0 : (int)diff.TotalMinutes;
        });
        int points = normalized.Sum(r => r.Points);

        var daily = normalized
            .GroupBy(r =>
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(r.StartUtc, tz);
                return DateOnly.FromDateTime(local);
            })
            .Select(g => new DailyMinutesPointDto
            {
                Date = g.Key,
                Minutes = g.Sum(r =>
                {
                    var end = r.EndUtc ?? r.StartUtc;
                    var diff = end - r.StartUtc;
                    return diff.TotalMinutes < 0 ? 0 : (int)diff.TotalMinutes;
                })
            })
            .OrderBy(x => x.Date)
            .ToList();

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



    [HttpGet("member/stats")]
    public async Task<ActionResult<MemberStatsDto>> MemberStats([FromQuery] Guid memberId)
    {
        if (memberId == Guid.Empty) return BadRequest("memberId is required.");

        var q = _db.TrainingSessions.AsNoTracking()
            .Where(s => s.MemberID == memberId && s.CompletedFlag && s.EndedAt != null);

        var totalSessions = await q.CountAsync();
        var totalMinutes = await q.SumAsync(s => EF.Functions.DateDiffMinute(s.StartedAt, s.EndedAt!.Value));
        var avg = totalSessions == 0 ? 0 : (double)totalMinutes / totalSessions;

        var firstAt = await q.MinAsync(s => (DateTime?)s.StartedAt);
        var lastAt = await q.MaxAsync(s => (DateTime?)s.EndedAt);

        // 台北時區、週一為週首
        TimeZoneInfo tz;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); }
        catch { tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); }

        DateTime ToWeekStartLocal(DateTime utc)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz).Date;
            int delta = ((int)local.DayOfWeek + 6) % 7; // Monday=0
            return local.AddDays(-delta);
        }

        // 先取回 UTC 時間（在資料庫端只做條件/投影，不做本地方法）
        var startedList = await q.Select(s => s.StartedAt).ToListAsync();

        // 再在記憶體中執行你的區域函式
        var weekStarts = startedList
            .Select(ToWeekStartLocal)
            .Distinct()
            .OrderBy(d => d)
            .ToList();


        int best = 0, cur = 0; DateTime? prev = null;
        foreach (var w in weekStarts)
        {
            cur = (prev == null || (w - prev.Value).TotalDays == 7) ? cur + 1 : 1;
            best = Math.Max(best, cur);
            prev = w;
        }

        // 目前連續週（含本週或上一週起點）
        int current = 0;
        if (weekStarts.Count > 0)
        {
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            int delta = ((int)todayLocal.DayOfWeek + 6) % 7;
            var thisWeekStart = todayLocal.AddDays(-delta);
            var lastWeekStart = thisWeekStart.AddDays(-7);

            for (int i = weekStarts.Count - 1; i >= 0; i--)
            {
                var w = weekStarts[i];
                if (current == 0 && (w == thisWeekStart || w == lastWeekStart))
                {
                    current = 1;
                    var j = i - 1; var expect = w.AddDays(-7);
                    while (j >= 0 && weekStarts[j] == expect) { current++; expect = expect.AddDays(-7); j--; }
                    break;
                }
            }
        }

        return Ok(new MemberStatsDto
        {
            MemberID = memberId,
            TotalSessions = totalSessions,
            TotalMinutes = totalMinutes,
            AvgMinutesPerSession = avg,
            CurrentWeeklyStreak = current,
            BestWeeklyStreak = best,
            FirstSessionAt = firstAt,
            LastSessionAt = lastAt
        });
    }
}

