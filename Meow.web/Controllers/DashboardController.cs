using Meow.Shared.Dtos;                 // MemberDto 的命名空間（依你的實際命名空間調整）
using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;       // AdminWeeklySummaryDto
using Meow.Shared.Dtos.TrainingSessions;
using Meow.Web.Areas.Admin.ViewModels;  // DashboardVm
using Meow.Web.Services;                // IBackendApi
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Meow.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBackendApi _api;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IBackendApi api, ILogger<DashboardController> logger)
        {
            _api = api;
            _logger = logger;
        }


        // 若有人誤打 /Dashboard，就導到 MyWeekly，避免「找不到 Index 視圖」
        [HttpGet]
        public IActionResult Index() => RedirectToAction(nameof(MyWeekly));

        public sealed class MyWeeklyVm
        {
            public MemberWeeklySummaryDto Weekly { get; init; } = new();
            public List<TrainingSessionListItemDto> Recent { get; init; } = new();
        }

        // 會員個人每週摘要頁
        [HttpGet]
        public async Task<IActionResult> MyWeekly(DateTime? week)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idStr, out var memberId)) return Forbid();

            var weeklyTask = _api.GetMemberWeeklySummaryAsync(memberId, week);
            var recentTask = _api.GetRecentSessionsAsync(memberId, 3);
            await Task.WhenAll(weeklyTask, recentTask);

            var vm = new MyWeeklyVm
            {
                Weekly = weeklyTask.Result,
                Recent = recentTask.Result
            };
            return View(vm);
        }


    }
}
