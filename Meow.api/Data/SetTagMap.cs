using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[PrimaryKey("SetId", "TagId")]
[Table("SetTagMap")]
public partial class SetTagMap
{
    [Key]
    public Guid SetId { get; set; }

    [Key]
    public Guid TagId { get; set; }

    [Precision(0)]
    public DateTime MappedAt { get; set; }

    [ForeignKey("SetId")]
    [InverseProperty("SetTagMaps")]
    public virtual TrainingSet Set { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("SetTagMaps")]
    public virtual Tag Tag { get; set; } = null!;
}
