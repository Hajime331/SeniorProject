using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.TrainingSets
{
    public class TrainingSetUpdateDto
    {
        [Required] public Guid SetId { get; set; }

        [Required, StringLength(120)] public string Name { get; set; }
        [Required, StringLength(50)] public string BodyPart { get; set; }
        [Required, StringLength(30)] public string Equipment { get; set; }

        public string? Difficulty { get; set; }
        public int? EstimatedDurationSec { get; set; }

        public List<Guid> TagIds { get; set; } = new();
        [MinLength(1)] public List<TrainingSetItemUpdateDto> Items { get; set; } = new();
    }
}
