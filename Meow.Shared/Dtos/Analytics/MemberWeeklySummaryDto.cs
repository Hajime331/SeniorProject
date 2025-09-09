using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Analytics
{
    public class MemberWeeklySummaryDto
    {
        public Guid MemberID { get; set; }
        public DateTime WeekStartUtc { get; set; }
        public DateTime WeekEndUtc { get; set; }
        public int Sessions { get; set; }
        public int Minutes { get; set; }
        public int? Points { get; set; }  // 週內累積點數（可選）
        public List<DailyMinutesPointDto> Daily { get; set; } = new();
    }
}
