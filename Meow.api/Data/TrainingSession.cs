using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TrainingSession
{
    public Guid SessionID { get; set; }

    public Guid MemberID { get; set; }

    public Guid SetID { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public bool CompletedFlag { get; set; }

    public string? Notes { get; set; }

    public int? CaloriesBurned { get; set; }

    public int? PointsAwarded { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual TrainingSet Set { get; set; } = null!;

    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();
}
