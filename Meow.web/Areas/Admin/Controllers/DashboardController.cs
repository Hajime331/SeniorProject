using Meow.Shared.Dtos;                 // MemberDto 的命名空間（依你的實際命名空間調整）
using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;       // AdminWeeklySummaryDto
using Meow.Web.Areas.Admin.ViewModels;  // DashboardVm
using Meow.Web.Services;                // IBackendApi
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IBackendApi _api;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IBackendApi api, ILogger<DashboardController> logger)
        {
            _api = api;
            _logger = logger;
        }


        // 單一入口（避免重複的 Index 造成 AmbiguousMatchException）
        // /Admin/Dashboard?week=2025-09-08&recentTake=5&topTake=5
        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? week, int recentTake = 5, int topTake = 5)
        {
            var countTask = _api.GetMembersCountAsync();                 // /api/Members/count
            var recentTask = _api.GetRecentMembersAsync(recentTake);      // /api/Members/recent?take=N
            var weeklyTask = _api.GetAdminWeeklySummaryAsync(week, topTake); // /api/Analytics/admin/weekly

            try
            {
                await Task.WhenAll(countTask, recentTask, weeklyTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 Dashboard 資料失敗");
                TempData["Error"] = "載入儀表板資料失敗，部分資訊可能不完整。";
            }

            // 如果其中任何一項失敗，就提供安全預設值避免 View 爆掉
            var vm = new DashboardVm
            {
                TotalMembers = countTask.IsCompletedSuccessfully ? countTask.Result : 0,
                RecentMembers = recentTask.IsCompletedSuccessfully ? recentTask.Result : new List<MemberDto>(),
                Weekly = weeklyTask.IsCompletedSuccessfully ? weeklyTask.Result : new AdminWeeklySummaryDto()
            };

            return View(vm);
        }

    }
}
