using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("AvatarCatalog")]
[Index("Category", Name = "IX_AvatarCatalog_Category")]
[Index("Status", Name = "IX_AvatarCatalog_Status")]
[Index("ImageUrl", Name = "UQ_AvatarCatalog_ImageUrl", IsUnique = true)]
public partial class AvatarCatalog
{
    [Key]
    public Guid AvatarID { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(200)]
    public string? Tags { get; set; }

    [StringLength(16)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Avatar")]
    public virtual ICollection<MemberProfile> MemberProfiles { get; set; } = new List<MemberProfile>();
}
