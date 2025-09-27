using Meow.Shared.Dtos.TrainingSets;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            // 1) 呼叫 API 取清單
            var sets = await _api.GetTrainingSetsAsync(
                keyword,
                string.IsNullOrWhiteSpace(status) ? "Active" : status,
                difficulty,
                tagId
            );

            // 2) 取篩選用資料（Tag / 難度清單）
            var tags = await _api.GetTagsAsync();
            var difficulties = new List<string> { "初階", "中階", "高階" }; // 依你實際枚舉調整

            // 3) 包成 VM
            var vm = new TrainingSetIndexVm
            {
                Keyword = keyword,
                Difficulty = difficulty,
                TagId = tagId,

                AllTags = tags,
                AllDifficulties = difficulties,

                Sets = sets
            };

            // 4) 回傳 VM（不要直接回 List）
            return View(vm);
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
            // 基本驗證
            if (string.IsNullOrWhiteSpace(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), "請輸入課表名稱。");
            if (vm.Items == null || vm.Items.Count == 0)
                ModelState.AddModelError("", "至少需要一筆課表項目。");

            if (!ModelState.IsValid)
            {
                // 回傳 View 前，把下拉/多選資料補齊
                ViewBag.AllTags = await _api.GetTagsAsync();
                // 若 View 會用到影片清單（自動完成等），也可提前載入
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }

            // 建立 Items DTO
            var itemDtos = (vm.Items ?? new List<TrainingSetItemEditViewModel>())
                .Select(i => new TrainingSetItemCreateDto(
                    i.VideoId,
                    i.OrderNo,
                    i.TargetReps,
                    i.RestSec,
                    i.Rounds
                ))
                .ToList();

            // 建立 Create DTO（CoverUrl 先 null；上傳走獨立端點）
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
                // 建立成功會回傳 detail（內有 SetID）
                var created = await _api.CreateTrainingSetAsync(createDto);

                // 若有上傳封面 → 呼叫上傳端點
                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                {
                    try
                    {
                        await _api.UploadTrainingSetCoverAsync(created.SetID, vm.CoverFile);
                        TempData["Ok"] = "已建立課表並上傳封面。";
                    }
                    catch (Exception ex)
                    {
                        TempData["Err"] = "課表已建立，但封面上傳失敗：" + ex.Message;
                    }
                }
                else
                {
                    TempData["Ok"] = "已建立課表。";
                }

                // ✅ 依你的需求：成功後跳轉到編輯頁
                return RedirectToAction(nameof(Edit), new { id = created.SetID });
            }
            catch (ApplicationException ex)
            {
                // API 回來的可讀錯誤
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
                ViewBag.AllVideos = await _api.GetTrainingVideosAsync(null, "Published", (string?)null);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "建立失敗：" + ex.Message);
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

            // Items → Update DTO（TrainingSetItemUpdateDto 是屬性型，請用物件初始化器）
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

            var dto = new TrainingSetUpdateDto(
                vm.SetId!.Value,
                vm.Name?.Trim() ?? "",
                vm.BodyPart ?? "全身",
                vm.Equipment ?? "無器材",
                vm.Difficulty,
                vm.EstimatedDurationSec,
                vm.TagIds ?? new List<Guid>(),
                items,
                null // CoverUrl 交由上傳端點處理
            );

            try
            {
                await _api.UpdateTrainingSetAsync(vm.SetId.Value, dto);

                // 有選檔案 → 呼叫上傳端點
                if (vm.CoverFile is not null && vm.CoverFile.Length > 0)
                {
                    try
                    {
                        var url = await _api.UploadTrainingSetCoverAsync(vm.SetId.Value, vm.CoverFile);
                        TempData["Ok"] = url is not null ? "封面已上傳並更新。" : "封面已上傳。";
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

                // 重新導回 Edit（PRG 模式，避免重送表單），會在頁面顯示訊息與最新縮圖
                return RedirectToAction(nameof(Edit), new { id = vm.SetId.Value });
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AllTags = await _api.GetTagsAsync();
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
                    VideoTitle = videos.FirstOrDefault(v => v.VideoId == i.VideoId)?.Title ?? "(未知影片)",
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
                TempData["Ok"] = "已刪除課表。";
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
                videoId = v.VideoId,     // 依你的 DTO 實際屬性命名調整
                title = v.Title,
                durationSec = v.DurationSec
            });
            return Json(result);
        }

    }

}
