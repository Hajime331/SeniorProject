using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Videos
{
    public class TrainingVideoUpdateDto
    {
        [Required] public Guid VideoId { get; set; }

        [Required, StringLength(150)] public string Title { get; set; }
        [Required, StringLength(50)] public string BodyPart { get; set; }
        [Required, Url, StringLength(500)] public string Url { get; set; }
        [Range(1, int.MaxValue)] public int DurationSec { get; set; }

        [Required, StringLength(16)] public string Status { get; set; } // Draft/Published/Archived
        public List<Guid> TagIds { get; set; } = new();
    }
}
