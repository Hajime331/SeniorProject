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
            if (string.IsNullOrWhiteSpace(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), "請輸入課表名稱。");
            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError("", "至少需要一筆課表項目。");

            if (!ModelState.IsValid)
            {
                ViewBag.AllTags = await _api.GetTagsAsync();
                return View(vm);
            }

            // 1) Items → Create DTO（用「建構子」！）
            var itemDtos = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select(i => new TrainingSetItemCreateDto(
                    i.VideoId,
                    i.OrderNo,
                    i.TargetReps,
                    i.RestSec,
                    i.Rounds
                ))
                .ToList();

            // 2) Create DTO（CoverUrl 先 null；之後用上傳端點）
            var createDto = new TrainingSetCreateDto(
                vm.Name!.Trim(),
                vm.BodyPart ?? "全身",
                vm.Equipment ?? "無器材",
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                itemDtos,
                null
            );

            try
            {
                var created = await _api.CreateTrainingSetAsync(createDto);

                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                    await _api.UploadTrainingSetCoverAsync(created.SetID, vm.CoverFile); // :contentReference[oaicite:5]{index=5}

                TempData["Ok"] = "已建立課表。";
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
                return View(vm);
            }
        }



        // GET: Admin/TrainingSets/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _api.GetTrainingSetAsync(id);
            if (dto is null) return NotFound();

            var detail = await _api.GetTrainingSetAsync(id);
            var vm = new TrainingSetEditViewModel
            {
                SetId = detail.SetID,
                Name = detail.Name,
                BodyPart = detail.BodyPart,
                Equipment = detail.Equipment,
                Difficulty = detail.Difficulty,
                EstimatedDurationSec = detail.EstimatedDurationSec,
                TagIds = detail.TagIds?.ToList() ?? new(),
                CoverUrl = detail.CoverUrl,
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
                return View(vm);
            }

            // 1) Items → Update DTO（用物件初始化器）
            var items = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select(i => new TrainingSetItemUpdateDto
                {
                    SetItemId = i.SetItemId,
                    VideoId = i.VideoId,
                    OrderNo = i.OrderNo,
                    TargetReps = i.TargetReps,
                    RestSec = i.RestSec,
                    Rounds = i.Rounds
                })
                .ToList();

            // 2) Update DTO；CoverUrl 先給 null，改由獨立上傳端點處理
            var dto = new TrainingSetUpdateDto(
                vm.SetId!.Value,
                vm.Name?.Trim() ?? "",
                vm.BodyPart ?? "全身",
                vm.Equipment ?? "無器材",
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                items,
                null
            );

            try
            {
                // ★ IBackendApi 目前可用的是這個簽名（id + dto）
                await _api.UpdateTrainingSetAsync(vm.SetId.Value, dto);  // ← 修正呼叫方式 :contentReference[oaicite:1]{index=1}

                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                    await _api.UploadTrainingSetCoverAsync(vm.SetId.Value, vm.CoverFile); // :contentReference[oaicite:2]{index=2}

                TempData["Ok"] = "已更新課表。";
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
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
