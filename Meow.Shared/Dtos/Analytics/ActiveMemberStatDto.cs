using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Analytics
{
    public class ActiveMemberStatDto
    {
        public Guid MemberID { get; set; }
        public string Nickname { get; set; } = default!;
        public int Sessions { get; set; }
        public int Minutes { get; set; }
    }
}
