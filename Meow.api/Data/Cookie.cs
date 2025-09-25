using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class Cookie
{
    public Guid CookieID { get; set; }

    public string Name { get; set; } = null!;

    public int Price { get; set; }

    public string? Rarity { get; set; }

    public bool IsLimited { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    public virtual ICollection<CookiePurchase> CookiePurchases { get; set; } = new List<CookiePurchase>();

    public virtual ICollection<CookieTagMap> CookieTagMaps { get; set; } = new List<CookieTagMap>();
}
