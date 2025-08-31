using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[PrimaryKey("MemberID", "ItemID")]
[Table("MemberItem")]
[Index("ItemID", Name = "IX_MemberItem_Item")]
[Index("MemberID", Name = "IX_MemberItem_Member")]
public partial class MemberItem
{
    [Key]
    public Guid MemberID { get; set; }

    [Key]
    public Guid ItemID { get; set; }

    [Precision(0)]
    public DateTime OwnedAt { get; set; }

    [ForeignKey("ItemID")]
    [InverseProperty("MemberItems")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("MemberID")]
    [InverseProperty("MemberItems")]
    public virtual Member Member { get; set; } = null!;
}
