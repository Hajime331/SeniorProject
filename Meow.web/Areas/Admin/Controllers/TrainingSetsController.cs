using Meow.Shared.Dtos.TrainingSets;
using Meow.Shared.Dtos.Videos;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TrainingSetsController : Controller
    {
        private readonly IBackendApi _api;
        public TrainingSetsController(IBackendApi api) => _api = api;

        // GET: Admin/TrainingSets
        public async Task<IActionResult> Index(
            string? keyword,
            string? status,
            string? difficulty,
            Guid? tagId)
        {
            var sets = await _api.GetTrainingSetsAsync(
                keyword,
                string.IsNullOrWhiteSpace(status) ? "Active" : status,
                difficulty,
                tagId);

            var tags = await _api.GetTagsAsync();
            var difficulties = new List<string> { "初階", "中階", "進階" };

            var vm = new TrainingSetIndexVm
            {
                Keyword = keyword,
                Difficulty = difficulty,
                TagId = tagId,
                AllTags = tags,
                AllDifficulties = difficulties,
                Sets = sets
            };

            return View(vm);
        }

        // GET: Admin/TrainingSets/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AllTags = await _api.GetTagsAsync();
            var videos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
            ViewBag.AllVideos = videos;

            var vm = new TrainingSetEditViewModel
            {
                Items = new List<TrainingSetItemEditViewModel>
                {
                    new TrainingSetItemEditViewModel { OrderNo = 1 }
                }
            };
            PopulateVideoTitles(vm.Items, videos);
            return View(vm);
        }

        // POST: Admin/TrainingSets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingSetEditViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), "請輸入課表名稱。");
            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "至少需要一個訓練項目。");

            if (!ModelState.IsValid)
            {
                await ReloadCreateEditLookups(vm);
                return View(vm);
            }

            var itemDtos = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select((i, idx) => new TrainingSetItemCreateDto(
                    i.VideoId,
                    i.OrderNo > 0 ? i.OrderNo : idx + 1,
                    i.TargetReps,
                    i.RestSec,
                    i.Rounds))
                .ToList();

            var createDto = new TrainingSetCreateDto(
                vm.Name.Trim(),
                vm.BodyPart ?? "未指定",
                vm.Equipment ?? "無",
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                itemDtos,
                null);

            try
            {
                var created = await _api.CreateTrainingSetAsync(createDto);

                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                {
                    try
                    {
                        await _api.UploadTrainingSetCoverAsync(created.SetID, vm.CoverFile);
                        TempData["Ok"] = "課表已建立，封面上傳完成。";
                    }
                    catch (Exception ex)
                    {
                        TempData["Err"] = "課表已建立，但封面上傳失敗：" + ex.Message;
                    }
                }
                else
                {
                    TempData["Ok"] = "課表已建立。";
                }

                return RedirectToAction(nameof(Edit), new { id = created.SetID });
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await ReloadCreateEditLookups(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "新增失敗：" + ex.Message);
                await ReloadCreateEditLookups(vm);
                return View(vm);
            }
        }

        // GET: Admin/TrainingSets/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _api.GetTrainingSetAsync(id);
            if (dto is null) return NotFound();

            var videos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
            var vm = new TrainingSetEditViewModel
            {
                SetId = dto.SetID,
                Name = dto.Name,
                BodyPart = dto.BodyPart ?? string.Empty,
                Equipment = dto.Equipment ?? string.Empty,
                Difficulty = dto.Difficulty,
                EstimatedDurationSec = dto.EstimatedDurationSec,
                TagIds = dto.TagIds?.ToList() ?? new List<Guid>(),
                CoverUrl = dto.CoverUrl,
                Items = dto.Items.Select(x => new TrainingSetItemEditViewModel
                {
                    SetItemId = x.SetItemId,
                    VideoId = x.VideoId,
                    OrderNo = x.OrderNo,
                    TargetReps = x.TargetReps,
                    RestSec = x.RestSec,
                    Rounds = x.Rounds,
                    VideoTitle = videos.FirstOrDefault(v => v.VideoId == x.VideoId)?.Title ?? string.Empty
                }).ToList()
            };

            if (vm.Items.Count == 0)
                vm.Items.Add(new TrainingSetItemEditViewModel { OrderNo = 1 });

            ViewBag.AllTags = await _api.GetTagsAsync();
            ViewBag.AllVideos = videos;
            return View(vm);
        }

        // POST: Admin/TrainingSets/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingSetEditViewModel vm)
        {
            if (vm.SetId is null || vm.SetId == Guid.Empty)
                ModelState.AddModelError(string.Empty, "缺少課表識別碼。");
            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "至少需要一個訓練項目。");

            if (!ModelState.IsValid)
            {
                await ReloadCreateEditLookups(vm);
                return View(vm);
            }

            var items = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select((i, idx) => new TrainingSetItemUpdateDto
                {
                    SetItemId = i.SetItemId,
                    VideoId = i.VideoId,
                    OrderNo = i.OrderNo > 0 ? i.OrderNo : idx + 1,
                    TargetReps = i.TargetReps,
                    RestSec = i.RestSec,
                    Rounds = i.Rounds
                })
                .ToList();

            var dto = new TrainingSetUpdateDto(
                vm.SetId!.Value,
                vm.Name?.Trim() ?? string.Empty,
                vm.BodyPart ?? "未指定",
                vm.Equipment ?? "無",
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                items,
                null);

            try
            {
                await _api.UpdateTrainingSetAsync(vm.SetId.Value, dto);

                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                {
                    try
                    {
                        var url = await _api.UploadTrainingSetCoverAsync(vm.SetId.Value, vm.CoverFile);
                        TempData["Ok"] = url is not null ? "封面已更新。" : "封面上傳完成。";
                    }
                    catch (Exception ex)
                    {
                        TempData["Err"] = "封面上傳失敗：" + ex.Message;
                    }
                }
                else
                {
                    TempData["Ok"] = "課表內容已更新。";
                }

                return RedirectToAction(nameof(Edit), new { id = vm.SetId.Value });
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await ReloadCreateEditLookups(vm);
                return View(vm);
            }
        }

        // GET: Admin/TrainingSets/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
                return NotFound();

            var dto = await _api.GetTrainingSetAsync(id);

            if (dto == null)
                return NotFound();

            var videos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);

            var vm = new TrainingSetDetailVm
            {
                SetId = dto.SetID,
                Name = dto.Name,
                BodyPart = dto.BodyPart,
                Equipment = dto.Equipment,
                Difficulty = dto.Difficulty,
                EstimatedDurationSec = dto.EstimatedDurationSec,
                Status = dto.Status,
                CoverUrl = dto.CoverUrl,
                TagIds = dto.TagIds,
                Items = dto.Items.Select(i => new TrainingSetItemDetailVm
                {
                    SetItemId = i.SetItemId,
                    VideoId = i.VideoId,
                    VideoTitle = videos.FirstOrDefault(v => v.VideoId == i.VideoId)?.Title ?? "(找不到影片)",
                    OrderNo = i.OrderNo,
                    TargetReps = i.TargetReps,
                    RestSec = i.RestSec,
                    Rounds = i.Rounds
                }).ToList()
            };

            return View(vm);
        }

        // POST: Admin/TrainingSets/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();
            try
            {
                await _api.DeleteTrainingSetAsync(id);
                TempData["Ok"] = "課表已刪除或已封存。";
            }
            catch (ApplicationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> SearchVideos(string? q)
        {
            var list = await _api.GetTrainingVideosAsync(q, "Published", (string?)null);
            var result = list.Select(v => new {
                videoId = v.VideoId,
                title = v.Title,
                durationSec = v.DurationSec
            });
            return Json(result);
        }

        private async Task ReloadCreateEditLookups(TrainingSetEditViewModel vm)
        {
            vm.Items ??= new List<TrainingSetItemEditViewModel>();
            if (vm.Items.Count == 0)
                vm.Items.Add(new TrainingSetItemEditViewModel { OrderNo = 1 });

            ViewBag.AllTags = await _api.GetTagsAsync();
            var videos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
            ViewBag.AllVideos = videos;
            PopulateVideoTitles(vm.Items, videos);
        }

        private static void PopulateVideoTitles(IEnumerable<TrainingSetItemEditViewModel>? items, IEnumerable<TrainingVideoListItemDto>? videos)
        {
            if (items == null || videos == null) return;
            var lookup = videos.GroupBy(v => v.VideoId)
                               .ToDictionary(g => g.Key, g => g.First().Title ?? string.Empty);

            foreach (var item in items)
            {
                if (item.VideoId != Guid.Empty && lookup.TryGetValue(item.VideoId, out var title))
                {
                    item.VideoTitle = title;
                }
            }
        }
    }
}

