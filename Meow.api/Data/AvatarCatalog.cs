using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class AvatarCatalog
{
    public Guid AvatarID { get; set; }

    public string Name { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? Category { get; set; }

    public string? Tags { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MemberProfile> MemberProfiles { get; set; } = new List<MemberProfile>();
}
