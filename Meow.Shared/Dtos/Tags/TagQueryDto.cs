using Meow.Shared.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Tags
{
    public class TagQueryDto : PagingQuery
    {
        public string? Category { get; set; }   // 選填
    }
}
