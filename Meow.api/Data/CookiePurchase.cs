using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CookiePurchase
{
    public Guid PurchaseID { get; set; }

    public Guid MemberID { get; set; }

    public Guid CookieID { get; set; }

    public int Cost { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Cookie Cookie { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
