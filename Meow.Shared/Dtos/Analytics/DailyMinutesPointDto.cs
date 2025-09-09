using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Analytics
{
    public class DailyMinutesPointDto
    {
        public DateOnly Date { get; set; }  // 以 UTC 邊界切日
        public int Minutes { get; set; }
    }
}
