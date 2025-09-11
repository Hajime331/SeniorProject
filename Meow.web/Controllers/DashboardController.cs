using Meow.Shared.Dtos;                 // MemberDto 的命名空間（依你的實際命名空間調整）
using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;       // AdminWeeklySummaryDto
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

        [HttpGet]
        public async Task<IActionResult> MyWeekly(DateTime? week)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idStr, out var memberId)) return Forbid();

            var dto = await _api.GetMemberWeeklySummaryAsync(memberId, week);
            return View(dto); // 對應 Views/Dashboard/MyWeekly.cshtml
        }
    }
}
