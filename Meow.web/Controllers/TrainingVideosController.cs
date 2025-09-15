using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingVideos;

[Authorize]
public class TrainingVideosController : Controller
{
    private readonly IBackendApi _api;
    public TrainingVideosController(IBackendApi api) => _api = api;

    // GET /TrainingVideos
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, string? status, List<Guid>? tagIds)
    {
        var tags = await _api.GetTagsAsync();
        var videos = await _api.GetTrainingVideosAsync(keyword, status, tagIds);

        var vm = new TrainingVideoIndexVm
        {
            Keyword = keyword,
            Status = status,
            SelectedTagIds = (tagIds ?? new List<Guid>()),

            // 這行是關鍵：把不同命名空間的 TagDto 映射成 Shared 版
            AllTags = tags.Select(t => new Meow.Shared.Dtos.Tags.TagDto(t.TagID, t.Name)).ToList(),

            Videos = videos
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
