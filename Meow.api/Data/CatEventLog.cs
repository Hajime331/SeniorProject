using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CatEventLog
{
    public Guid LogID { get; set; }

    public Guid MemberCatID { get; set; }

    public Guid EventID { get; set; }

    public DateTime OccurredAt { get; set; }

    public string? RewardJson { get; set; }

    public string? TriggerSource { get; set; }

    public Guid? FeedID { get; set; }

    public virtual CookieFeedLog? CookieFeedLog { get; set; }

    public virtual CatEvent Event { get; set; } = null!;

    public virtual CookieFeedLog? Feed { get; set; }

    public virtual MemberCat MemberCat { get; set; } = null!;

    public virtual PointTransaction? PointTransaction { get; set; }
}
