using Meow.Shared.Dtos.TrainingSets;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class TrainingSetsController : Controller
{
    private readonly IBackendApi _api;
    public TrainingSetsController(IBackendApi api) => _api = api;

    // GET /TrainingSets
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, string? difficulty, Guid? tagId)
    {
        var sets = await _api.GetTrainingSetsAsync(keyword, "Active", difficulty, tagId);
        var allTags = await _api.GetTagsAsync();

        var vm = new TrainingSetIndexVm
        {
            Keyword = keyword ?? "",
            Status = "Active",
            Difficulty = difficulty,               // ← 帶進 VM
            TagId = tagId,
            AllTags = allTags.ToList(),
            AllDifficulties = new List<string> { "初階", "中階", "高階" }, // 若你已有來源可替換
            Sets = sets.ToList()
        };
        return View(vm);
    }





    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Start(Guid id, string? note)
        => RedirectToAction("Start", "TrainingSessions", new { setId = id, note });


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var tags = await _api.GetTagsAsync();
        var videos = await _api.GetTrainingVideosAsync(keyword: null, status: "Published", tagIds: null);
        var vm = new TrainingSetCreateVm
        {
            AllTags = tags.ToList(),
            AllVideos = videos.ToList(),
            Items = new List<TrainingSetItemInputVm> { new TrainingSetItemInputVm() }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingSetCreateVm model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "請輸入名稱");
        if (model.Items == null || model.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "請至少新增 1 個訓練項目");

        if (!ModelState.IsValid)
        {
            model.AllTags = (await _api.GetTagsAsync()).ToList();
            
            model.AllVideos = (await _api.GetTrainingVideosAsync(null, "Published", tagIdsCsv: null)).ToList();

            return View(model);
        }

        var itemDtos = model.Items.Select((item, idx) => new TrainingSetItemCreateDto(
            item.VideoId,
            item.OrderNo.HasValue ? item.OrderNo : idx + 1,
            item.TargetReps,
            item.RestSec,
            item.Rounds)).ToList();

        var dto = new TrainingSetCreateDto(
            model.Name.Trim(),
            model.BodyPart ?? "全身",
            model.Equipment ?? "無器材",
            model.Difficulty,
            model.EstimatedDurationSec,
            model.SelectedTagIds ?? new List<Guid>(),
            itemDtos,
            null
        );

        await _api.CreateTrainingSetAsync(dto);
        TempData["Ok"] = "已建立課表。";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _api.GetTrainingSetAsync(id);

        if (dto == null) return NotFound();

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

}
