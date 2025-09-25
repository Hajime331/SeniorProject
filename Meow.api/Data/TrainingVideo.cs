using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TrainingVideo
{
    public Guid VideoID { get; set; }

    public string Title { get; set; } = null!;

    public string BodyPart { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int DurationSec { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedByMemberID { get; set; }

    public string? ThumbnailUrl { get; set; }

    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();

    public virtual ICollection<TrainingSetItem> TrainingSetItems { get; set; } = new List<TrainingSetItem>();

    public virtual ICollection<VideoTagMap> VideoTagMaps { get; set; } = new List<VideoTagMap>();
}
