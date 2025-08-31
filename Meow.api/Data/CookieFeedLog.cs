using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("CookieFeedLog")]
[Index("MemberID", Name = "IX_CookieFeedLog_Member")]
[Index("MemberCatID", Name = "IX_CookieFeedLog_MemberCat")]
[Index("MemberCatID", "FedAt", Name = "IX_CookieFeedLog_MemberCat_Time", IsDescending = new[] { false, true })]
[Index("MemberID", "FedAt", Name = "IX_CookieFeedLog_Member_Time", IsDescending = new[] { false, true })]
public partial class CookieFeedLog
{
    [Key]
    public Guid FeedID { get; set; }

    public Guid MemberID { get; set; }

    public Guid MemberCatID { get; set; }

    public Guid CookieID { get; set; }

    [Precision(0)]
    public DateTime FedAt { get; set; }

    public Guid? EventLogID { get; set; }

    [InverseProperty("Feed")]
    public virtual CatEventLog? CatEventLog { get; set; }

    [ForeignKey("CookieID")]
    [InverseProperty("CookieFeedLogs")]
    public virtual Cookie Cookie { get; set; } = null!;

    [ForeignKey("EventLogID")]
    [InverseProperty("CookieFeedLog")]
    public virtual CatEventLog? EventLog { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("CookieFeedLogs")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("MemberCatID")]
    [InverseProperty("CookieFeedLogs")]
    public virtual MemberCat MemberCat { get; set; } = null!;
}
