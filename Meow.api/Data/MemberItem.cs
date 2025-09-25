using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class MemberItem
{
    public Guid MemberID { get; set; }

    public Guid ItemID { get; set; }

    public DateTime OwnedAt { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
