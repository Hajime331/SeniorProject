using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("MemberProfile")]
public partial class MemberProfile
{
    [Key]
    public Guid MemberID { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    public Guid? AvatarID { get; set; }

    [Precision(0)]
    public DateTime AvatarUpdatedAt { get; set; }

    public int? HeightCm { get; set; }

    public int? WeightKg { get; set; }

    [ForeignKey("AvatarID")]
    [InverseProperty("MemberProfiles")]
    public virtual AvatarCatalog? Avatar { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("MemberProfile")]
    public virtual Member Member { get; set; } = null!;
}
