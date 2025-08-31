using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("MemberCat")]
[Index("MemberID", Name = "IX_MemberCat_Member")]
[Index("MemberID", "CatSpeciesID", Name = "UQ_MemberCat_Member_Species", IsUnique = true)]
public partial class MemberCat
{
    [Key]
    public Guid MemberCatID { get; set; }

    public Guid MemberID { get; set; }

    public Guid CatSpeciesID { get; set; }

    [StringLength(80)]
    public string? Nickname { get; set; }

    [Precision(0)]
    public DateTime UnlockedAt { get; set; }

    public bool IsInRoom { get; set; }

    [StringLength(16)]
    public string? Status { get; set; }

    [InverseProperty("MemberCat")]
    public virtual ICollection<CatEventLog> CatEventLogs { get; set; } = new List<CatEventLog>();

    [ForeignKey("CatSpeciesID")]
    [InverseProperty("MemberCats")]
    public virtual CatSpecy CatSpecies { get; set; } = null!;

    [InverseProperty("MemberCat")]
    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    [ForeignKey("MemberID")]
    [InverseProperty("MemberCats")]
    public virtual Member Member { get; set; } = null!;
}
