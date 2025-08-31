using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("Item")]
public partial class Item
{
    [Key]
    public Guid ItemID { get; set; }

    [StringLength(120)]
    public string Name { get; set; } = null!;

    [StringLength(30)]
    public string ItemType { get; set; } = null!;

    [StringLength(16)]
    public string Rarity { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<MemberItem> MemberItems { get; set; } = new List<MemberItem>();
}
