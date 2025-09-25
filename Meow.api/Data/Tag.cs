using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class Tag
{
    public Guid TagID { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public virtual ICollection<CookieTagMap> CookieTagMaps { get; set; } = new List<CookieTagMap>();

    public virtual ICollection<SetTagMap> SetTagMaps { get; set; } = new List<SetTagMap>();

    public virtual ICollection<VideoTagMap> VideoTagMaps { get; set; } = new List<VideoTagMap>();
}
