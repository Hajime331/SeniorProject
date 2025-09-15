using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;
using Meow.Web.ViewModels.TrainingSets;

[Authorize]
public class TrainingSetsController : Controller
{
    private readonly IBackendApi _api;
    public TrainingSetsController(IBackendApi api) => _api = api;

    // GET /TrainingSets
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var sets = await _api.GetTrainingSetsAsync(keyword, "Active");

        var vm = new TrainingSetIndexVm
        {
            Keyword = keyword,
            Sets = sets
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Start(Guid id, string? note)
    {
        // 轉到既有 TrainingSessionsController.Start
        return RedirectToAction("Start", "TrainingSessions", new { setId = id, note });
    }
}
