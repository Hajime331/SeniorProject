using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TrainingSetItem")]
[Index("SetID", Name = "IX_SetItem_Set")]
[Index("SetID", "OrderNo", Name = "UQ_TrainingSetItem_Set_Order", IsUnique = true)]
public partial class TrainingSetItem
{
    [Key]
    public Guid SetItemID { get; set; }

    public Guid SetID { get; set; }

    public Guid VideoID { get; set; }

    public int OrderNo { get; set; }

    public int? TargetReps { get; set; }

    public int? RestSec { get; set; }

    public int? Rounds { get; set; }

    [ForeignKey("SetID")]
    [InverseProperty("TrainingSetItems")]
    public virtual TrainingSet Set { get; set; } = null!;

    [InverseProperty("SetItem")]
    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();

    [ForeignKey("VideoID")]
    [InverseProperty("TrainingSetItems")]
    public virtual TrainingVideo Video { get; set; } = null!;
}
