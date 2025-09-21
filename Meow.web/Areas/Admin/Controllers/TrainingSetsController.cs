using System.Net.Http.Json;
using Meow.Shared.Dtos.TrainingSets;
using Meow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TrainingSetsController : Controller
    {
        private readonly IBackendApi _api;
        public TrainingSetsController(IBackendApi api) => _api = api;

        // GET: Admin/TrainingSets
        public async Task<IActionResult> Index(string? keyword, string? status = "Active")
        {
            var list = await _api.GetTrainingSetsAsync(keyword, status);
            return View(list.ToList());
        }

        // GET: Admin/TrainingSets/Create
        public async Task<IActionResult> Create()
        {
            // 若你的 View 需要 Tag/Video 下拉，這裡先載入
            ViewBag.AllTags = await _api.GetTagsAsync();
            // 避免多載模稜兩可：第三參數指定 (string?)null
            ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
            return View(new TrainingSetEditViewModel());
        }

        // POST: Admin/TrainingSets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingSetEditViewModel vm)
        {
            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError("", "至少需要一筆課表項目。");

            if (!ModelState.IsValid)
            {
                ViewBag.AllTags = await _api.GetTagsAsync();
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }

            var itemDtos = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select((i, idx) => new TrainingSetItemCreateDto(
                    i.VideoId,
                    (int?)i.OrderNo ?? (idx + 1), // 這裡用 (int?) 避免 ?? 編譯錯
                    i.TargetReps, i.RestSec, i.Rounds
                )).ToList();

            var dto = new TrainingSetCreateDto(
                vm.Name, vm.BodyPart, vm.Equipment, vm.Difficulty,
                vm.EstimatedDurationSec, vm.TagIds ?? new List<Guid>(), itemDtos
            );

            try
            {
                var created = await _api.CreateTrainingSetAsync(dto);
                TempData["Ok"] = "已建立課表。";
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }
        }

        // GET: Admin/TrainingSets/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _api.GetTrainingSetAsync(id);
            if (dto is null) return NotFound();

            var vm = new TrainingSetEditViewModel
            {
                SetId = dto.SetID,
                Name = dto.Name,
                BodyPart = dto.BodyPart,
                Equipment = dto.Equipment,
                Difficulty = dto.Difficulty,
                EstimatedDurationSec = dto.EstimatedDurationSec,
                TagIds = dto.TagIds ?? new List<Guid>(),
                Items = dto.Items.Select(x => new TrainingSetItemEditViewModel
                {
                    SetItemId = x.SetItemId,
                    VideoId = x.VideoId,
                    OrderNo = x.OrderNo,
                    TargetReps = x.TargetReps,
                    RestSec = x.RestSec,
                    Rounds = x.Rounds
                }).ToList()
            };

            ViewBag.AllTags = await _api.GetTagsAsync();
            ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
            return View(vm);
        }

        // POST: Admin/TrainingSets/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingSetEditViewModel vm)
        {
            if (vm.SetId is null || vm.SetId == Guid.Empty)
                ModelState.AddModelError("", "缺少 SetId。");

            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError("", "至少需要一筆課表項目。");

            if (!ModelState.IsValid)
            {
                ViewBag.AllTags = await _api.GetTagsAsync();
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }

            var items = vm.Items.Select(i => new TrainingSetItemUpdateDto
            {
                SetItemId = i.SetItemId,
                VideoId = i.VideoId,
                OrderNo = i.OrderNo,
                TargetReps = i.TargetReps,
                RestSec = i.RestSec,
                Rounds = i.Rounds
            }).ToList();

            var dto = new TrainingSetUpdateDto(
                vm.SetId.Value, vm.Name, vm.BodyPart, vm.Equipment, vm.Difficulty,
                vm.EstimatedDurationSec, vm.TagIds ?? new List<Guid>(), items
            );

            try
            {
                var updated = await _api.UpdateTrainingSetAsync(vm.SetId.Value, dto);
                TempData["Ok"] = "已更新課表。";
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }
        }

        // POST: Admin/TrainingSets/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _api.DeleteTrainingSetAsync(id);
                TempData["Ok"] = "已刪除課表。";
            }
            catch (ApplicationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }

}
