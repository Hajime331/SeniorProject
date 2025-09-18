using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TrainingSet")]
public partial class TrainingSet
{
    [Key]
    public Guid SetId { get; set; }

    [StringLength(120)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string BodyPart { get; set; } = null!;

    [StringLength(30)]
    public string Equipment { get; set; } = null!;

    [StringLength(16)]
    public string? Difficulty { get; set; }

    public int? EstimatedDurationSec { get; set; }

    public bool IsCustom { get; set; }

    public Guid? OwnerMemberId { get; set; }

    [StringLength(16)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("OwnerMemberID")]
    [InverseProperty("TrainingSets")]
    public virtual Member? OwnerMember { get; set; }

    [InverseProperty("Set")]
    public virtual ICollection<SetTagMap> SetTagMaps { get; set; } = new List<SetTagMap>();

    [InverseProperty("Set")]
    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    [InverseProperty("Set")]
    public virtual ICollection<TrainingSetItem> TrainingSetItems { get; set; } = new List<TrainingSetItem>();

}
