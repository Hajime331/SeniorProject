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


    // GET /TrainingSessions/Detail/{id}   
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "缺少必要的參數：id。";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var dto = await _api.GetTrainingSessionAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "找不到這筆訓練紀錄。";
                return RedirectToAction(nameof(Index));
            }
            return View(dto); // 要把 dto 丟進 View
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"讀取訓練明細失敗：{ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }


    // POST /TrainingSessions/Complete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, int? calories, int? points, string? note)
    {
        if (id == Guid.Empty) return BadRequest();

        var dto = new TrainingSessionCompleteDto
        {
            EndedAt = DateTime.UtcNow,   // 直接以現在時間結束
            CompletedFlag = true,
            CaloriesBurned = calories,
            PointsAwarded = points,
            Notes = note
        };

        try
        {
            var updated = await _api.CompleteTrainingSessionAsync(id, dto);
            TempData["Success"] = "本次訓練已結束。";
            return RedirectToAction(nameof(Detail), new { id = updated.SessionID });
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"結束訓練失敗：{ex.Message}";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(TrainingSessionItemUpdateDto dto)
    {
        if (dto.SessionItemID == Guid.Empty)
        {
            TempData["Error"] = "缺少 SessionItemID。";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var updated = await _api.UpdateTrainingSessionItemAsync(dto);
            TempData["Success"] = $"步驟 #{updated.OrderNo} 更新成功。";
            return RedirectToAction(nameof(Detail), new { id = dto.SessionID }); // SessionID 需由 View 帶回來
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"更新失敗：{ex.Message}";
            return RedirectToAction(nameof(Detail), new { id = dto.SessionID });
        }
    }

 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItemAjax([FromForm] TrainingSessionItemUpdateDto dto)
    {
        if (dto.SessionItemID == Guid.Empty || dto.SessionID == Guid.Empty)
            return BadRequest(new { ok = false, error = "缺少必要參數。" });

        try
        {
            var updated = await _api.UpdateTrainingSessionItemAsync(dto);
            return Json(new
            {
                ok = true,
                item = new
                {
                    updated.SessionItemID,
                    updated.OrderNo,
                    updated.Status,
                    updated.ActualReps,
                    updated.ActualWeight,
                    updated.ActualDurationSec,
                    updated.ActualRestSec,
                    updated.RoundsDone,
                    updated.Note,
                    updated.VideoTitle
                }
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { ok = false, error = $"更新失敗：{ex.Message}" });
        }
    }

}
