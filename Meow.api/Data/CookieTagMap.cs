using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CookieTagMap
{
    public Guid CookieID { get; set; }

    public Guid TagID { get; set; }

    public DateTime MappedAt { get; set; }

    public virtual Cookie Cookie { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
