using Meow.Shared.Dtos;
using Meow.Web.Models;

namespace Meow.Web.Areas.Admin.Models
{
    public class DashboardVm
    {
        public int TotalMembers { get; set; }

        // MemberDto 已在前台 Members 清單使用；這裡直接沿用
        public List<MemberDto> RecentMembers { get; set; } = new();
    }
}