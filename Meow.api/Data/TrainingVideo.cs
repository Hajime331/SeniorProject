using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TrainingVideo")]
public partial class TrainingVideo
{
    [Key]
    public Guid VideoId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(50)]
    public string BodyPart { get; set; } = null!;

    [StringLength(500)]
    public string Url { get; set; } = null!;

    public int DurationSec { get; set; }

    [StringLength(16)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Video")]
    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();

    [InverseProperty("Video")]
    public virtual ICollection<TrainingSetItem> TrainingSetItems { get; set; } = new List<TrainingSetItem>();

    [InverseProperty("Video")]
    public virtual ICollection<VideoTagMap> VideoTagMaps { get; set; } = new List<VideoTagMap>();
}
