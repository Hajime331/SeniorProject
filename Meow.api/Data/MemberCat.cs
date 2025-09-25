using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class MemberCat
{
    public Guid MemberCatID { get; set; }

    public Guid MemberID { get; set; }

    public Guid CatSpeciesID { get; set; }

    public string? Nickname { get; set; }

    public DateTime UnlockedAt { get; set; }

    public bool IsInRoom { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<CatEventLog> CatEventLogs { get; set; } = new List<CatEventLog>();

    public virtual CatSpecy CatSpecies { get; set; } = null!;

    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    public virtual Member Member { get; set; } = null!;
}
