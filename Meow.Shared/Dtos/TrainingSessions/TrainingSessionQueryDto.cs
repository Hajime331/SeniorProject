using Meow.Shared.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionQueryDto : PagingQuery
    {
        public Guid? MemberId { get; set; }
        public Guid? SetId { get; set; }
        // 若有日期區間，直接沿用 PagingQuery 的 From/To
    }
}
