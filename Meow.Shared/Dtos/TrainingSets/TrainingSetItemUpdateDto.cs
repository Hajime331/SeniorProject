using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.TrainingSets
{
    public class TrainingSetItemUpdateDto
    {
        public Guid? SetItemId { get; set; } // 新增的項目可為 null
        [Required] public Guid VideoId { get; set; }
        public int OrderNo { get; set; }
        public int? TargetReps { get; set; }
        public int? RestSec { get; set; }
        public int? Rounds { get; set; }
    }
}
