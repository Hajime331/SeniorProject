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
    public async Task<IActionResult> Index(string? keyword, string? difficulty, Guid? tagId)
    {
        var sets = await _api.GetTrainingSetsAsync(keyword, "Active", difficulty, tagId);
        var tags = await _api.GetTagsAsync();

        var vm = new TrainingSetIndexVm
        {
            Keyword = keyword,
            Difficulty = difficulty,
            TagId = tagId,
            AllTags = tags,
            // AllDifficulties 如有需要也可塞（固定 3~4 種字串）
            Sets = sets // ← 關鍵：List 包進去
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
        var set = await _api.GetTrainingSetAsync(id);
        if (set == null) return NotFound();
        var tags = await _api.GetTagsAsync();
        // 取出所有影片清單，以便顯示項目中的影片名稱
        var videos = await _api.GetTrainingVideosAsync(null, null, tagIdsCsv: null);
        var vm = new TrainingSetDetailVm
        {
            Set = set,
            AllTags = tags.ToList(),
            AllVideos = videos.ToList()
        };
        return View(vm);
    }
}
