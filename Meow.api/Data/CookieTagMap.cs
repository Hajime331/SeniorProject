using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[PrimaryKey("CookieID", "TagID")]
[Table("CookieTagMap")]
public partial class CookieTagMap
{
    [Key]
    public Guid CookieID { get; set; }

    [Key]
    public Guid TagID { get; set; }

    [Precision(0)]
    public DateTime MappedAt { get; set; }

    [ForeignKey("CookieID")]
    [InverseProperty("CookieTagMaps")]
    public virtual Cookie Cookie { get; set; } = null!;

    [ForeignKey("TagID")]
    [InverseProperty("CookieTagMaps")]
    public virtual Tag Tag { get; set; } = null!;
}
