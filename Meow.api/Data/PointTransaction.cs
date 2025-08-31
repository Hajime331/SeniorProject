using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("PointTransaction")]
[Index("CreatedAt", Name = "IX_PointTransaction_CreatedAt")]
[Index("MemberID", Name = "IX_PointTransaction_Member")]
[Index("MemberID", "CreatedAt", Name = "IX_PointTransaction_Member_Signed")]
public partial class PointTransaction
{
    [Key]
    public Guid TxnID { get; set; }

    public Guid MemberID { get; set; }

    [StringLength(8)]
    public string Direction { get; set; } = null!;

    public int Amount { get; set; }

    [StringLength(20)]
    public string Source { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public Guid? RefID { get; set; }

    public Guid? EventLogID { get; set; }

    public int? SignedAmount { get; set; }

    [ForeignKey("EventLogID")]
    [InverseProperty("PointTransaction")]
    public virtual CatEventLog? EventLog { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("PointTransactions")]
    public virtual Member Member { get; set; } = null!;
}
