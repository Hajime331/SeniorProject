using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TrainingSession")]
[Index("MemberID", "StartedAt", Name = "IX_TrainingSession_Member_StartedAt", IsDescending = new[] { false, true })]
[Index("SetID", Name = "IX_TrainingSession_Set")]
public partial class TrainingSession
{
    [Key]
    public Guid SessionID { get; set; }

    public Guid MemberID { get; set; }

    public Guid SetID { get; set; }

    [Precision(0)]
    public DateTime StartedAt { get; set; }

    [Precision(0)]
    public DateTime? EndedAt { get; set; }

    public bool CompletedFlag { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int? CaloriesBurned { get; set; }

    public int? PointsAwarded { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("TrainingSessions")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("SetID")]
    [InverseProperty("TrainingSessions")]
    public virtual TrainingSet Set { get; set; } = null!;

    [InverseProperty("Session")]
    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();
}
