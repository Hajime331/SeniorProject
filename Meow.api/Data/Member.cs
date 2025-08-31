using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("Member")]
[Index("Email", Name = "UQ_Member_Email", IsUnique = true)]
[Index("EmailNormalized", Name = "UQ_Member_EmailNormalized", IsUnique = true)]
public partial class Member
{
    [Key]
    public Guid MemberID { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string? EmailNormalized { get; set; }

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(80)]
    public string Nickname { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public bool IsAdmin { get; set; }

    [StringLength(16)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime? LastLoginAt { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    [InverseProperty("Member")]
    public virtual ICollection<CookiePurchase> CookiePurchases { get; set; } = new List<CookiePurchase>();

    [InverseProperty("Member")]
    public virtual ICollection<MemberCat> MemberCats { get; set; } = new List<MemberCat>();

    [InverseProperty("Member")]
    public virtual ICollection<MemberItem> MemberItems { get; set; } = new List<MemberItem>();

    [InverseProperty("Member")]
    public virtual MemberPoint? MemberPoint { get; set; }

    [InverseProperty("Member")]
    public virtual MemberProfile? MemberProfile { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    [InverseProperty("Member")]
    public virtual ICollection<TimerTemplate> TimerTemplates { get; set; } = new List<TimerTemplate>();

    [InverseProperty("Member")]
    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    [InverseProperty("OwnerMember")]
    public virtual ICollection<TrainingSet> TrainingSets { get; set; } = new List<TrainingSet>();
}
