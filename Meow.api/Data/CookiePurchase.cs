using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("CookiePurchase")]
[Index("CookieID", Name = "IX_CookiePurchase_Cookie")]
[Index("MemberID", Name = "IX_CookiePurchase_Member")]
public partial class CookiePurchase
{
    [Key]
    public Guid PurchaseID { get; set; }

    public Guid MemberID { get; set; }

    public Guid CookieID { get; set; }

    public int Cost { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("CookieID")]
    [InverseProperty("CookiePurchases")]
    public virtual Cookie Cookie { get; set; } = null!;

    [ForeignKey("MemberID")]
    [InverseProperty("CookiePurchases")]
    public virtual Member Member { get; set; } = null!;
}
