using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Videos
{
    public class TrainingVideoListItemDto
    {
        public Guid VideoId { get; set; }
        public string Title { get; set; }
        public string BodyPart { get; set; }
        public string Url { get; set; }
        public int DurationSec { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();
    }
}
