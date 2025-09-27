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
    public async Task<IActionResult> Index(string? keyword, Guid? tagId)
    {
        var tags = await _api.GetTagsAsync();

        string? tagIdsCsv = tagId.HasValue ? tagId.Value.ToString() : null;

        // 前台固定顯示 Published
        var videos = await _api.GetTrainingVideosAsync(
            string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
            "Published",
            tagIdsCsv
        );

        var vm = new TrainingVideoIndexVm
        {
            Keyword = keyword,
            Status = "Published",
            SelectedTagIds = tagId.HasValue ? new List<Guid> { tagId.Value } : new List<Guid>(),
            AllTags = tags.ToList(),
            Videos = videos
        };

        return View(vm);
    }

    // 影片詳情（前台可看）
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _api.GetTrainingVideoAsync(id);
        if (dto == null) return NotFound();
        return View(dto); // View 直接吃 TrainingVideoDetailDto
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
