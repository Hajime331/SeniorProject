using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CatPattern
{
    public string PatternCode { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? SpriteKey { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CatSpecy> CatSpecies { get; set; } = new List<CatSpecy>();
}
