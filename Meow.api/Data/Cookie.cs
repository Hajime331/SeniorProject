using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("Cookie")]
public partial class Cookie
{
    [Key]
    public Guid CookieID { get; set; }

    [StringLength(120)]
    public string Name { get; set; } = null!;

    public int Price { get; set; }

    [StringLength(16)]
    public string? Rarity { get; set; }

    public bool IsLimited { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Cookie")]
    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    [InverseProperty("Cookie")]
    public virtual ICollection<CookiePurchase> CookiePurchases { get; set; } = new List<CookiePurchase>();

    [InverseProperty("Cookie")]
    public virtual ICollection<CookieTagMap> CookieTagMaps { get; set; } = new List<CookieTagMap>();
}
