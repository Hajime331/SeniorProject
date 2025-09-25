using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class MemberProfile
{
    public Guid MemberID { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Gender { get; set; }

    public Guid? AvatarID { get; set; }

    public DateTime AvatarUpdatedAt { get; set; }

    public int? HeightCm { get; set; }

    public int? WeightKg { get; set; }

    public virtual AvatarCatalog? Avatar { get; set; }

    public virtual Member Member { get; set; } = null!;
}
