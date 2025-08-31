using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("CatEventLog")]
[Index("EventID", "OccurredAt", Name = "IX_CatEventLog_Event_Time", IsDescending = new[] { false, true })]
[Index("MemberCatID", "OccurredAt", Name = "IX_CatEventLog_MemberCat_Time", IsDescending = new[] { false, true })]
public partial class CatEventLog
{
    [Key]
    public Guid LogID { get; set; }

    public Guid MemberCatID { get; set; }

    public Guid EventID { get; set; }

    [Precision(0)]
    public DateTime OccurredAt { get; set; }

    public string? RewardJson { get; set; }

    [StringLength(20)]
    public string? TriggerSource { get; set; }

    public Guid? FeedID { get; set; }

    [InverseProperty("EventLog")]
    public virtual CookieFeedLog? CookieFeedLog { get; set; }

    [ForeignKey("EventID")]
    [InverseProperty("CatEventLogs")]
    public virtual CatEvent Event { get; set; } = null!;

    [ForeignKey("FeedID")]
    [InverseProperty("CatEventLog")]
    public virtual CookieFeedLog? Feed { get; set; }

    [ForeignKey("MemberCatID")]
    [InverseProperty("CatEventLogs")]
    public virtual MemberCat MemberCat { get; set; } = null!;

    [InverseProperty("EventLog")]
    public virtual PointTransaction? PointTransaction { get; set; }
}
