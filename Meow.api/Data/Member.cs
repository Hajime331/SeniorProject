using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class Member
{
    public Guid MemberID { get; set; }

    public string Email { get; set; } = null!;

    public string? EmailNormalized { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Nickname { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsAdmin { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<CookieFeedLog> CookieFeedLogs { get; set; } = new List<CookieFeedLog>();

    public virtual ICollection<CookiePurchase> CookiePurchases { get; set; } = new List<CookiePurchase>();

    public virtual ICollection<MemberCat> MemberCats { get; set; } = new List<MemberCat>();

    public virtual ICollection<MemberItem> MemberItems { get; set; } = new List<MemberItem>();

    public virtual MemberPoint? MemberPoint { get; set; }

    public virtual MemberProfile? MemberProfile { get; set; }

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual ICollection<TimerTemplate> TimerTemplates { get; set; } = new List<TimerTemplate>();

    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    public virtual ICollection<TrainingSet> TrainingSets { get; set; } = new List<TrainingSet>();
}
