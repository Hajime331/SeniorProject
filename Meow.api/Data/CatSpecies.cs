using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CatSpecies
{
    public Guid CatSpeciesID { get; set; }

    public string Name { get; set; } = null!;

    public string PatternCode { get; set; } = null!;

    public string PersonalityCode { get; set; } = null!;

    public string Rarity { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<MemberCat> MemberCat { get; set; } = new List<MemberCat>();

    public virtual CatPattern PatternCodeNavigation { get; set; } = null!;
}
