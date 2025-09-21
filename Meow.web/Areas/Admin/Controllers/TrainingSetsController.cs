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
        public IActionResult Create()
        {
            return View(new TrainingSetEditViewModel());
        }

        // POST: Admin/TrainingSets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingSetEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new TrainingSetCreateDto(
                vm.Name,
                vm.BodyPart,
                vm.Equipment,
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                (vm.Items ?? new List<TrainingSetItemEditViewModel>()).Select(i => new TrainingSetItemCreateDto(
                    i.VideoId,
                    i.OrderNo,
                    i.TargetReps,
                    i.RestSec,
                    i.Rounds
                )).ToList()
            );

            var created = await _api.CreateTrainingSetAsync(dto);
            // 介面是回 DetailDto；若要顯示成功提示可用 TempData
            return RedirectToAction(nameof(Index));
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
            return View(vm);
        }

        // POST: Admin/TrainingSets/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingSetEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (vm.SetId is null) { ModelState.AddModelError("", "缺少 SetId。"); return View(vm); }

            var dto = new TrainingSetUpdateDto(
                vm.SetId.Value,
                vm.Name,
                vm.BodyPart,
                vm.Equipment,
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                (vm.Items ?? new List<TrainingSetItemEditViewModel>()).Select(i => new TrainingSetItemUpdateDto
                {
                    SetItemId = i.SetItemId,
                    VideoId = i.VideoId,
                    OrderNo = i.OrderNo,
                    TargetReps = i.TargetReps,
                    RestSec = i.RestSec,
                    Rounds = i.Rounds
                }).ToList()
            );

            // 若你的 IBackendApi 只有「dto-only」版本：改成 await _api.UpdateTrainingSetAsync(dto);
            var updated = await _api.UpdateTrainingSetAsync(vm.SetId.Value, dto);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/TrainingSets/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _api.DeleteTrainingSetAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
