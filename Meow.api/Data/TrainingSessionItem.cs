using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TrainingSessionItem")]
[Index("SessionID", Name = "IX_TrainingSessionItem_Session")]
[Index("SessionID", "OrderNo", Name = "UQ_SessionItem_Session_Order", IsUnique = true)]
public partial class TrainingSessionItem
{
    [Key]
    public Guid SessionItemID { get; set; }

    public Guid SessionID { get; set; }

    public Guid SetItemID { get; set; }

    public Guid VideoId { get; set; }

    public int OrderNo { get; set; }

    [StringLength(16)]
    public string Status { get; set; } = null!;

    public int? ActualReps { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? ActualWeight { get; set; }

    public int? ActualDurationSec { get; set; }

    public int? ActualRestSec { get; set; }

    public int? RoundsDone { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("SessionID")]
    [InverseProperty("TrainingSessionItems")]
    public virtual TrainingSession Session { get; set; } = null!;

    [ForeignKey("SetItemID")]
    [InverseProperty("TrainingSessionItems")]
    public virtual TrainingSetItem SetItem { get; set; } = null!;

    [ForeignKey("VideoId")]
    [InverseProperty("TrainingSessionItems")]
    public virtual TrainingVideo Video { get; set; } = null!;
}
