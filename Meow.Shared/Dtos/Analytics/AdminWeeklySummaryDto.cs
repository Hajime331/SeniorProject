using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Analytics
{
    public class AdminWeeklySummaryDto
    {
        public DateTime WeekStartUtc { get; set; }   // 週起（UTC）
        public DateTime WeekEndUtc { get; set; }     // 週迄（UTC, 開區間）
        public int TotalSessions { get; set; }       // 本週完成場次
        public int TotalMinutes { get; set; }        // 本週總分鐘（完成場次）
        public int ActiveMembers { get; set; }       // 有完成場次的會員數
        public List<ActiveMemberStatDto> TopActive { get; set; } = new();
    }
}
