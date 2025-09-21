using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[PrimaryKey("SetID", "TagID")]
[Table("SetTagMap")]
public partial class SetTagMap
{
    [Key]
    public Guid SetID { get; set; }

    [Key]
    public Guid TagID { get; set; }

    [Precision(0)]
    public DateTime MappedAt { get; set; }

    [ForeignKey("SetID")]
    [InverseProperty("SetTagMaps")]
    public virtual TrainingSet Set { get; set; } = null!;

    [ForeignKey("TagID")]
    [InverseProperty("SetTagMaps")]
    public virtual Tag Tag { get; set; } = null!;
}
