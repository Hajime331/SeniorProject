using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class Item
{
    public Guid ItemID { get; set; }

    public string Name { get; set; } = null!;

    public string ItemType { get; set; } = null!;

    public string Rarity { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<MemberItem> MemberItems { get; set; } = new List<MemberItem>();
}
