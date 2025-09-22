using Meow.Shared.Dtos.Videos;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingVideos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize] // 仍保留：若將來加「收藏/按讚」等需登入功能
public class TrainingVideosController : Controller
{
    private readonly IBackendApi _api;
    public TrainingVideosController(IBackendApi api) => _api = api;

    // 首頁列表（前台可看）：預設只顯示 Published
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, string? status, List<Guid>? tagIds)
    {
        // 預設狀態 Published；若你想固定只顯示 Published，可直接寫死 "Published"
        var targetStatus = string.IsNullOrWhiteSpace(status) ? "Published" : status.Trim();

        // 讀取所有可用標籤（做篩選 UI）
        var tags = await _api.GetTagsAsync();

        // 第三個參數避免多載模糊：有 tagIds 時轉 CSV；沒有就 (string?)null
        string? tagIdsCsv = (tagIds != null && tagIds.Any())
            ? string.Join(",", tagIds)
            : (string?)null;

        var videos = await _api.GetTrainingVideosAsync(
            string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
            targetStatus,
            tagIdsCsv
        );

        var vm = new TrainingVideoIndexVm
        {
            Keyword = keyword,
            Status = targetStatus,
            SelectedTagIds = tagIds ?? new List<Guid>(),
            AllTags = tags.ToList(),
            Videos = videos // IEnumerable<TrainingVideoListItemDto>
        };

        return View(vm);
    }

    // 影片詳情（前台可看）
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

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

    // 後續若有需要登入的操作（例如更新狀態），保留 [Authorize]
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
