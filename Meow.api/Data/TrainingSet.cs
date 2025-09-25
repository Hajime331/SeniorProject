using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TrainingSet
{
    public Guid SetID { get; set; }

    public string Name { get; set; } = null!;

    public string BodyPart { get; set; } = null!;

    public string Equipment { get; set; } = null!;

    public string? Difficulty { get; set; }

    public int? EstimatedDurationSec { get; set; }

    public bool IsCustom { get; set; }

    public Guid? OwnerMemberID { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CoverUrl { get; set; }

    public virtual Member? OwnerMember { get; set; }

    public virtual ICollection<SetTagMap> SetTagMaps { get; set; } = new List<SetTagMap>();

    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    public virtual ICollection<TrainingSetItem> TrainingSetItems { get; set; } = new List<TrainingSetItem>();
}
