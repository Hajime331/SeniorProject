using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("Tag")]
[Index("Category", Name = "IX_Tag_Category")]
[Index("Name", Name = "UQ_Tag_Name", IsUnique = true)]
public partial class Tag
{
    [Key]
    public Guid TagID { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string Category { get; set; } = null!;

    [InverseProperty("Tag")]
    public virtual ICollection<CookieTagMap> CookieTagMaps { get; set; } = new List<CookieTagMap>();

    [InverseProperty("Tag")]
    public virtual ICollection<SetTagMap> SetTagMaps { get; set; } = new List<SetTagMap>();

    [InverseProperty("Tag")]
    public virtual ICollection<VideoTagMap> VideoTagMaps { get; set; } = new List<VideoTagMap>();
}
