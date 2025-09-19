using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[PrimaryKey("VideoId", "TagID")]
[Table("VideoTagMap")]
public partial class VideoTagMap
{
    [Key]
    public Guid VideoId { get; set; }

    [Key]
    public Guid TagID { get; set; }

    [Precision(0)]
    public DateTime MappedAt { get; set; }

    [ForeignKey("TagID")]
    [InverseProperty("VideoTagMaps")]
    public virtual Tag Tag { get; set; } = null!;

    [ForeignKey("VideoId")]
    [InverseProperty("VideoTagMaps")]
    public virtual TrainingVideo Video { get; set; } = null!;
}
