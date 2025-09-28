using Meow.Shared.Dtos.Videos;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingVideos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TrainingVideosController : Controller
    {
        private readonly IBackendApi _api;
        public TrainingVideosController(IBackendApi api) => _api = api;

        // GET: Admin/TrainingVideos
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status, Guid? tagId)
        {
            var targetStatus = string.IsNullOrWhiteSpace(status) ? "" : status.Trim();
            var tags = await _api.GetTagsAsync();

            string? tagIdsCsv = tagId.HasValue ? tagId.Value.ToString() : null;

            var videos = await _api.GetTrainingVideosAsync(
                string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
                string.IsNullOrWhiteSpace(targetStatus) ? null : targetStatus,
                tagIdsCsv
            );

            var vm = new TrainingVideoIndexVm
            {
                Keyword = keyword,
                Status = targetStatus,
                SelectedTagIds = tagId.HasValue ? new List<Guid> { tagId.Value } : new List<Guid>(),
                AllTags = tags.ToList(),
                Videos = videos
            };
            return View(vm);
        }

        // GET: Admin/TrainingVideos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.AllTags = await _api.GetTagsAsync();      // ← 必填
            return View(new TrainingVideoEditViewModel { TagIds = new List<Guid>() });
        }

        // POST: Admin/TrainingVideos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingVideoEditViewModel vm, IFormFile? ThumbnailFile)
        {
            ViewBag.AllTags = await _api.GetTagsAsync();
            vm.TagIds ??= new List<Guid>();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var createDto = new TrainingVideoCreateDto(
                    vm.Title?.Trim() ?? "",
                    string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                    vm.Url?.Trim() ?? "",
                    vm.DurationSec,
                    string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                    vm.TagIds,
                    string.IsNullOrWhiteSpace(vm.ThumbnailUrl) ? null : vm.ThumbnailUrl!.Trim()
                );

                var created = await _api.CreateTrainingVideoAsync(createDto);
                Guid newId = created.VideoId;

                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    var newUrl = await _api.UploadTrainingVideoThumbnailAsync(newId, ThumbnailFile);
                    if (!string.IsNullOrWhiteSpace(newUrl))
                    {
                        // 立刻寫回 DB
                        var writeBack = new TrainingVideoUpdateDto(
                            newId,
                            vm.Title?.Trim() ?? "",
                            string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                            vm.Url?.Trim() ?? "",
                            vm.DurationSec,
                            string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                            vm.TagIds,
                            newUrl
                        );
                        await _api.UpdateTrainingVideoAsync(writeBack);
                        TempData["Ok"] = "已建立影片並上傳封面。";
                    }
                    else
                    {
                        TempData["Err"] = "影片已建立，但封面上傳失敗。";
                    }
                }
                else
                {
                    TempData["Ok"] = "已建立影片。";
                }

                return RedirectToAction(nameof(Edit), new { id = newId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "建立失敗：" + ex.Message);
                return View(vm);
            }
        }



        // GET: Admin/TrainingVideos/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _api.GetTrainingVideoAsync(id);
            if (dto == null) return NotFound();

            ViewBag.AllTags = await _api.GetTagsAsync();   // ← 必填
            var vm = new TrainingVideoEditViewModel
            {
                VideoId = dto.VideoId,
                Title = dto.Title ?? "",
                BodyPart = dto.BodyPart ?? "",
                Url = dto.Url ?? "",
                DurationSec = dto.DurationSec,
                Status = dto.Status ?? "Draft",
                TagIds = dto.TagIds?.ToList() ?? new List<Guid>(),  // ← 防 null
                ThumbnailUrl = dto.ThumbnailUrl
            };
            return View(vm);
        }

        // POST: Admin/TrainingVideos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingVideoEditViewModel vm, IFormFile? ThumbnailFile)
        {
            // 無論如何，回傳 View 時都需要的選單資料
            ViewBag.AllTags = await _api.GetTagsAsync();
            vm.TagIds ??= new List<Guid>();

            if (!ModelState.IsValid)
            {
                return View(vm); // ← 直接返回，已確保 return
            }

            try
            {
                // 1) 先更新基本欄位（含文字輸入的 ThumbnailUrl）
                var updateDto = new TrainingVideoUpdateDto(
                    vm.VideoId,
                    vm.Title?.Trim() ?? "",
                    string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                    vm.Url?.Trim() ?? "",
                    vm.DurationSec,
                    string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                    vm.TagIds,
                    string.IsNullOrWhiteSpace(vm.ThumbnailUrl) ? null : vm.ThumbnailUrl!.Trim()
                );

                await _api.UpdateTrainingVideoAsync(updateDto);

                // 2) 若有檔案 → 上傳，並把新 URL 回寫 DB
                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    var newUrl = await _api.UploadTrainingVideoThumbnailAsync(vm.VideoId, ThumbnailFile);
                    if (!string.IsNullOrWhiteSpace(newUrl))
                    {
                        var writeBack = new TrainingVideoUpdateDto(
                            vm.VideoId,
                            vm.Title?.Trim() ?? "",
                            string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                            vm.Url?.Trim() ?? "",
                            vm.DurationSec,
                            string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                            vm.TagIds,
                            newUrl // ★把上傳回來的 URL 寫回
                        );
                        await _api.UpdateTrainingVideoAsync(writeBack);
                        TempData["Ok"] = "封面已上傳並更新。";
                    }
                    else
                    {
                        TempData["Err"] = "封面上傳失敗。";
                    }
                }
                else
                {
                    TempData["Ok"] = "已儲存變更。";
                }

                // PRG：回 GET 維持預覽
                return RedirectToAction(nameof(Edit), new { id = vm.VideoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "儲存失敗：" + ex.Message);
                return View(vm); // ← 確保 return
            }
        }



        // GET: Admin/TrainingVideos/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _api.GetTrainingVideoAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }



        // POST: Admin/TrainingVideos/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _api.DeleteTrainingVideoAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
