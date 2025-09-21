using Meow.Shared.Dtos.Videos;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingVideos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var tags = await _api.GetTagsAsync();
        var vm = new TrainingVideoCreateVm
        {
            AllTags = tags.ToList(),
            Status = "Draft"
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingVideoCreateVm model)
    {
        // 基本驗證
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "請輸入標題");
        if (string.IsNullOrWhiteSpace(model.BodyPart))
            ModelState.AddModelError(nameof(model.BodyPart), "請選擇部位");
        if (string.IsNullOrWhiteSpace(model.Url))
            ModelState.AddModelError(nameof(model.Url), "請輸入影片連結");
        if (model.DurationSec <= 0)
            ModelState.AddModelError(nameof(model.DurationSec), "請輸入正確的秒數");
        if (!ModelState.IsValid)
        {
            model.AllTags = (await _api.GetTagsAsync()).ToList();
            return View(model);
        }

        var dto = new TrainingVideoCreateDto(
            model.Title.Trim(),
            model.BodyPart.Trim(),
            model.Url.Trim(),
            model.DurationSec,
            model.Status.Trim(),
            model.SelectedTagIds ?? new List<Guid>(),
            model.ThumbnailUrl
        );

        await _api.CreateTrainingVideoAsync(dto);
        TempData["Ok"] = "已建立影片。";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var video = await _api.GetTrainingVideoAsync(id);
        if (video == null) return NotFound();
        var tags = await _api.GetTagsAsync();
        var vm = new TrainingVideoDetailVm
        {
            Video = video,
            AllTags = tags.ToList()
        };
        return View(vm);
    }
}
