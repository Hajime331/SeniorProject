using System.Net.Http.Json;
using Meow.Shared.Dtos.Videos;
using Meow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TrainingVideosController : Controller
    {
        private readonly IBackendApi _api;
        public TrainingVideosController(IBackendApi api) => _api = api;

        // GET: Admin/TrainingVideos
        public async Task<IActionResult> Index(string? keyword, string? status)
        {
            var list = await _api.GetTrainingVideosAsync(keyword, status, null);
            return View(list.ToList());
        }

        // GET: Admin/TrainingVideos/Create
        public IActionResult Create()
        {
            return View(new TrainingVideoEditViewModel());
        }

        // POST: Admin/TrainingVideos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingVideoEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new TrainingVideoCreateDto(
                vm.Title,
                vm.BodyPart,
                vm.Url,
                vm.DurationSec,
                vm.Status,
                vm.TagIds ?? new List<Guid>(),
                vm.ThumbnailUrl
            );

            var created = await _api.CreateTrainingVideoAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/TrainingVideos/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _api.GetTrainingVideoAsync(id);
            if (dto is null) return NotFound();

            var vm = new TrainingVideoEditViewModel
            {
                VideoId = dto.VideoId,
                Title = dto.Title,
                BodyPart = dto.BodyPart,
                Url = dto.Url,
                DurationSec = dto.DurationSec,
                Status = dto.Status,
                TagIds = dto.TagIds?.ToList() ?? new List<Guid>(),
                ThumbnailUrl = dto.ThumbnailUrl
            };
            return View(vm);
        }

        // POST: Admin/TrainingVideos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingVideoEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (vm.VideoId == Guid.Empty) { ModelState.AddModelError("", "缺少 VideoId。"); return View(vm); }

            var dto = new TrainingVideoUpdateDto(
                vm.VideoId,
                vm.Title,
                vm.BodyPart,
                vm.Url,
                vm.DurationSec,
                vm.Status,
                vm.TagIds ?? new List<Guid>()
            );

            var updated = await _api.UpdateTrainingVideoAsync(dto);
            return RedirectToAction(nameof(Index));
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
