using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CookieFeedLog
{
    public Guid FeedID { get; set; }

    public Guid MemberID { get; set; }

    public Guid MemberCatID { get; set; }

    public Guid CookieID { get; set; }

    public DateTime FedAt { get; set; }

    public Guid? EventLogID { get; set; }

    public virtual CatEventLog? CatEventLog { get; set; }

    public virtual Cookie Cookie { get; set; } = null!;

    public virtual CatEventLog? EventLog { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual MemberCat MemberCat { get; set; } = null!;
}
