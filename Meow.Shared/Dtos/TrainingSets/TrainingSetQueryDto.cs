using Meow.Shared.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.TrainingSets
{
    public class TrainingSetQueryDto : PagingQuery
    {
        public string? Status { get; set; }     // Active / Archived
        public string? BodyPart { get; set; }
        public string? Equipment { get; set; }
        public Guid? TagId { get; set; }
        public Guid? OwnerMemberId { get; set; }
    }
}
