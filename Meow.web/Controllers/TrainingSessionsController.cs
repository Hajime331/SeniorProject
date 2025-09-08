using Meow.Shared.Dtos.TrainingSessions;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingSessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class TrainingSessionsController : Controller
{
    private readonly IBackendApi _api;

    public TrainingSessionsController(IBackendApi api)
    {
        _api = api;
    }

    // GET /TrainingSessions?from=2025-09-01&to=2025-09-08&page=1&pageSize=12
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int page = 1, int pageSize = 12)
    {
        // 從 Claims 抓目前登入者的 MemberID（NameIdentifier）
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var memberId))
        {
            return Forbid(); // 理論上不會；保險起見
        }

        var paged = await _api.GetTrainingSessionsAsync(memberId, from, to, page, pageSize);

        var vm = new TrainingSessionListVm
        {
            Items = paged.Items,
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize,
            From = from,
            To = to
        };

        return View(vm);
    }

    // POST /TrainingSessions/Start?setId=...
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid setId, string? note)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var memberId)) return Forbid();
        if (setId == Guid.Empty) return BadRequest();

        var dto = new TrainingSessionCreateDto
        {
            SetID = setId,
            Notes = note
        };

        var created = await _api.StartTrainingSessionAsync(memberId, dto);

        TempData["Success"] = "已開始一場新的訓練。";
        // 你之後會實作 Detail 頁；先導向 Detail，比較符合「開始後立即操作」
        return RedirectToAction(nameof(Detail), new { id = created.SessionID });
    }

    // 之後我們會實作 Detail（本回合先佔位）
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        // TODO: 呼叫 GET /api/TrainingSessions/{id} 取明細（稍後帶你做）
        return View(); // 先讓路由存在
    }
}
