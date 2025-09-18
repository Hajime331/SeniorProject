using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingVideos;

[Authorize]
public class TrainingVideosController : Controller
{
    private readonly IBackendApi _api;
    public TrainingVideosController(IBackendApi api)
    {
        _api = api;
    }

    // GET /TrainingVideos
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, string? status, List<Guid>? tagIds)
    {
        var tags = await _api.GetTagsAsync(); // 直接使用 Shared 的 TagDto
        var videos = await _api.GetTrainingVideosAsync(keyword, status, tagIds);

        var vm = new TrainingVideoIndexVm
        {
            Keyword = keyword,
            Status = status,
            SelectedTagIds = tagIds ?? new List<Guid>(),
            AllTags = tags.ToList(),      // 不再做 TagID/TagId 轉換
            Videos = videos               // List<TrainingVideoListItemDto>
        };

        return View(vm);
    }

    // POST /TrainingVideos/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status, string? returnUrl = null)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(status))
        {
            TempData["Error"] = "缺少必要參數。";
            return RedirectToAction(nameof(Index));
        }

        await _api.UpdateTrainingVideoStatusAsync(id, status.Trim());
        TempData["Ok"] = "已更新影片狀態。";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
