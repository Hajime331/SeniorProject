using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;
using Meow.Web.Models;

namespace Meow.Web.Areas.Admin.ViewModels
{
    public class DashboardVm
    {
        public int TotalMembers { get; set; }

        // MemberDto 已在前台 Members 清單使用；這裡直接沿用
        public List<MemberDto> RecentMembers { get; set; } = new();

        // 新增：訓練統計
        public AdminWeeklySummaryDto Weekly { get; init; } = new();
    }
}