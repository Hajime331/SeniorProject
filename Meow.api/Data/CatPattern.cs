using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("CatPattern")]
[Index("DisplayName", Name = "UQ_CatPattern_DisplayName", IsUnique = true)]
public partial class CatPattern
{
    [Key]
    [StringLength(50)]
    public string PatternCode { get; set; } = null!;

    [StringLength(120)]
    public string DisplayName { get; set; } = null!;

    [StringLength(200)]
    public string? SpriteKey { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("PatternCodeNavigation")]
    public virtual ICollection<CatSpecy> CatSpecies { get; set; } = new List<CatSpecy>();
}
