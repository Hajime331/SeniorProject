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
        public string Title { get; set; } = "";
        public string BodyPart { get; set; } = "";
        public string Url { get; set; } = "";
        public int DurationSec { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Guid> TagIds { get; set; } = new();
        public string? ThumbnailUrl { get; set; } // ★ 新增
    }
}
