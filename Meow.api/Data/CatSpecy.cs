using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

public partial class CatSpecy
{
    [Key]
    public Guid CatSpeciesID { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string PatternCode { get; set; } = null!;

    [StringLength(50)]
    public string PersonalityCode { get; set; } = null!;

    [StringLength(16)]
    public string Rarity { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("CatSpecies")]
    public virtual ICollection<MemberCat> MemberCats { get; set; } = new List<MemberCat>();

    [ForeignKey("PatternCode")]
    [InverseProperty("CatSpecies")]
    public virtual CatPattern PatternCodeNavigation { get; set; } = null!;
}
