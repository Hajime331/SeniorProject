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
            if (!ModelState.IsValid)
            {
                ViewBag.AllTags = await _api.GetTagsAsync();  // ← 回傳 View 前要再塞
                vm.TagIds ??= new List<Guid>();               // ← 防 null
                return View(vm);
            }

            var createDto = new TrainingVideoCreateDto(
                vm.Title.Trim(),
                string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                vm.Url.Trim(),
                vm.DurationSec,
                string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                vm.TagIds ?? new List<Guid>(),
                string.IsNullOrWhiteSpace(vm.ThumbnailUrl) ? null : vm.ThumbnailUrl.Trim()
            );

            var createdVideo = await _api.CreateTrainingVideoAsync(createDto);
            Guid newId = createdVideo.VideoId;

            if (ThumbnailFile != null && ThumbnailFile.Length > 0)
            {
                await _api.UploadTrainingVideoThumbnailAsync(newId, ThumbnailFile); // ← 直接丟 Guid
            }

            TempData["Ok"] = "已建立影片。";
            return RedirectToAction(nameof(Edit), new { id = newId });
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
            if (!ModelState.IsValid)
            {
                ViewBag.AllTags = await _api.GetTagsAsync();  // ← 回傳 View 前要再塞
                vm.TagIds ??= new List<Guid>();               // ← 防 null
                return View(vm);
            }

            // 更新基本欄位（包含 ThumbnailUrl 文字）
            var updateDto = new TrainingVideoUpdateDto(
                vm.VideoId,
                vm.Title.Trim(),
                string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                vm.Url.Trim(),
                vm.DurationSec,
                string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status,
                vm.TagIds ?? new List<Guid>(),
                string.IsNullOrWhiteSpace(vm.ThumbnailUrl) ? null : vm.ThumbnailUrl.Trim()
            );

            await _api.UpdateTrainingVideoAsync(updateDto);


            // 若有檔案：上傳並嘗試寫回 DB 的 ThumbnailUrl
            if (ThumbnailFile != null && ThumbnailFile.Length > 0)
            {
                var newUrl = await _api.UploadTrainingVideoThumbnailAsync(vm.VideoId, ThumbnailFile);

                if (!string.IsNullOrWhiteSpace(newUrl))
                {
                    // 再用新的 URL 更新一次（確保 DB 有值）
                    var updateThumbOnly = new TrainingVideoUpdateDto(
                        vm.VideoId,
                        vm.Title?.Trim() ?? "",
                        string.IsNullOrWhiteSpace(vm.BodyPart) ? null : vm.BodyPart.Trim(),
                        vm.Url?.Trim() ?? "",
                        vm.DurationSec,
                        vm.Status ?? "Draft",
                        vm.TagIds ?? new List<Guid>(),
                        newUrl // 寫回新的縮圖網址
                    );
                    await _api.UpdateTrainingVideoAsync(updateThumbOnly);

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


            // PRG：回到同頁保留預覽
            return RedirectToAction(nameof(Edit), new { id = vm.VideoId });
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
