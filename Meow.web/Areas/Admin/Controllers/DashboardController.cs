using Meow.Shared.Dtos.Accounts;
using Meow.Web.Areas.Admin.Models;   // 使用到 DashboardVm
using Meow.Web.Models;
using Meow.Web.Services;             // 使用到 IBackendApi
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]                   // 這個 Controller 屬於 Admin 這個 Area
    [Authorize(Policy = "AdminOnly")] // 只有 IsAdmin=True 的使用者能進來
    public class DashboardController : Controller
    {
        private readonly IBackendApi _api;
        private readonly ILogger<DashboardController> _logger;

        // 透過 DI 取得呼叫後端 API 的服務
        public DashboardController(IBackendApi api, ILogger<DashboardController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // 後台首頁動作方法
        public async Task<IActionResult> Index()
        {
            // 1) 併發呼叫，縮短等待時間
            var countTask = _api.GetMemberCountAsync();
            var recentTask = _api.GetRecentMembersAsync(5);

            int total = 0;
            var recent = new List<MemberDto>();

            try
            {
                await Task.WhenAll(countTask, recentTask);
                total = countTask.Result;
                recent = recentTask.Result ?? new List<MemberDto>();
            }
            catch (Exception ex)
            {
                // 2) 失敗時記 Log + 溫和退場（頁面仍可顯示）
                _logger.LogError(ex, "載入 Dashboard 資料失敗");
                TempData["Error"] = "載入儀表板資料失敗，部分資訊可能不完整。";
            }

            var vm = new DashboardVm
            {
                TotalMembers = total,
                RecentMembers = recent
            };

            return View(vm);
        }
    }
}
