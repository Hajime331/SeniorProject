using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AvatarCatalog> AvatarCatalogs { get; set; }

    public virtual DbSet<CatEvent> CatEvents { get; set; }

    public virtual DbSet<CatEventLog> CatEventLogs { get; set; }

    public virtual DbSet<CatPattern> CatPatterns { get; set; }

    public virtual DbSet<CatSpecy> CatSpecies { get; set; }

    public virtual DbSet<Cookie> Cookies { get; set; }

    public virtual DbSet<CookieFeedLog> CookieFeedLogs { get; set; }

    public virtual DbSet<CookiePurchase> CookiePurchases { get; set; }

    public virtual DbSet<CookieTagMap> CookieTagMaps { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberCat> MemberCats { get; set; }

    public virtual DbSet<MemberItem> MemberItems { get; set; }

    public virtual DbSet<MemberPoint> MemberPoints { get; set; }

    public virtual DbSet<MemberProfile> MemberProfiles { get; set; }

    public virtual DbSet<PointTransaction> PointTransactions { get; set; }

    public virtual DbSet<SetTagMap> SetTagMaps { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TimerTemplate> TimerTemplates { get; set; }

    public virtual DbSet<TrainingSession> TrainingSessions { get; set; }

    public virtual DbSet<TrainingSessionItem> TrainingSessionItems { get; set; }

    public virtual DbSet<TrainingSet> TrainingSets { get; set; }

    public virtual DbSet<TrainingSetItem> TrainingSetItems { get; set; }

    public virtual DbSet<TrainingVideo> TrainingVideos { get; set; }

    public virtual DbSet<VideoTagMap> VideoTagMaps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvatarCatalog>(entity =>
        {
            entity.HasKey(e => e.AvatarID);

            entity.ToTable("AvatarCatalog");

            entity.HasIndex(e => e.Category, "IX_AvatarCatalog_Category");

            entity.HasIndex(e => e.Status, "IX_AvatarCatalog_Status");

            entity.HasIndex(e => e.ImageUrl, "UQ_AvatarCatalog_ImageUrl").IsUnique();

            entity.Property(e => e.AvatarID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasDefaultValue("Active");
            entity.Property(e => e.Tags).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CatEvent>(entity =>
        {
            entity.HasKey(e => e.EventID);

            entity.ToTable("CatEvent");

            entity.HasIndex(e => e.Code, "UQ_CatEvent_Code").IsUnique();

            entity.Property(e => e.EventID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code).HasMaxLength(60);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(120);
        });

        modelBuilder.Entity<CatEventLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.ToTable("CatEventLog");

            entity.HasIndex(e => new { e.EventID, e.OccurredAt }, "IX_CatEventLog_Event_Time").IsDescending(false, true);

            entity.HasIndex(e => new { e.MemberCatID, e.OccurredAt }, "IX_CatEventLog_MemberCat_Time").IsDescending(false, true);

            entity.HasIndex(e => e.FeedID, "UX_CatEventLog_FeedID")
                .IsUnique()
                .HasFilter("([FeedID] IS NOT NULL)");

            entity.Property(e => e.LogID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.OccurredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TriggerSource).HasMaxLength(20);

            entity.HasOne(d => d.Event).WithMany(p => p.CatEventLogs)
                .HasForeignKey(d => d.EventID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CatEventLog_Event");

            entity.HasOne(d => d.Feed).WithOne(p => p.CatEventLog)
                .HasForeignKey<CatEventLog>(d => d.FeedID)
                .HasConstraintName("FK_CatEventLog_Feed");

            entity.HasOne(d => d.MemberCat).WithMany(p => p.CatEventLogs)
                .HasForeignKey(d => d.MemberCatID)
                .HasConstraintName("FK_CatEventLog_MemberCat");
        });

        modelBuilder.Entity<CatPattern>(entity =>
        {
            entity.HasKey(e => e.PatternCode);

            entity.ToTable("CatPattern");

            entity.HasIndex(e => e.DisplayName, "UQ_CatPattern_DisplayName").IsUnique();

            entity.Property(e => e.PatternCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DisplayName).HasMaxLength(120);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.SpriteKey).HasMaxLength(200);
        });

        modelBuilder.Entity<CatSpecy>(entity =>
        {
            entity.HasKey(e => e.CatSpeciesID);

            entity.Property(e => e.CatSpeciesID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.PatternCode).HasMaxLength(50);
            entity.Property(e => e.PersonalityCode).HasMaxLength(50);
            entity.Property(e => e.Rarity).HasMaxLength(16);

            entity.HasOne(d => d.PatternCodeNavigation).WithMany(p => p.CatSpecies)
                .HasForeignKey(d => d.PatternCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CatSpecies_Pattern");
        });

        modelBuilder.Entity<Cookie>(entity =>
        {
            entity.ToTable("Cookie");

            entity.Property(e => e.CookieID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.Rarity).HasMaxLength(16);
        });

        modelBuilder.Entity<CookieFeedLog>(entity =>
        {
            entity.HasKey(e => e.FeedID);

            entity.ToTable("CookieFeedLog");

            entity.HasIndex(e => e.MemberID, "IX_CookieFeedLog_Member");

            entity.HasIndex(e => e.MemberCatID, "IX_CookieFeedLog_MemberCat");

            entity.HasIndex(e => new { e.MemberCatID, e.FedAt }, "IX_CookieFeedLog_MemberCat_Time").IsDescending(false, true);

            entity.HasIndex(e => new { e.MemberID, e.FedAt }, "IX_CookieFeedLog_Member_Time").IsDescending(false, true);

            entity.HasIndex(e => e.EventLogID, "UX_CookieFeedLog_EventLog")
                .IsUnique()
                .HasFilter("([EventLogID] IS NOT NULL)");

            entity.Property(e => e.FeedID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookieFeedLogs)
                .HasForeignKey(d => d.CookieID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_Cookie");

            entity.HasOne(d => d.EventLog).WithOne(p => p.CookieFeedLog)
                .HasForeignKey<CookieFeedLog>(d => d.EventLogID)
                .HasConstraintName("FK_CookieFeedLog_EventLog");

            entity.HasOne(d => d.MemberCat).WithMany(p => p.CookieFeedLogs)
                .HasForeignKey(d => d.MemberCatID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_MemberCat");

            entity.HasOne(d => d.Member).WithMany(p => p.CookieFeedLogs)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_Member");
        });

        modelBuilder.Entity<CookiePurchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseID);

            entity.ToTable("CookiePurchase");

            entity.HasIndex(e => e.CookieID, "IX_CookiePurchase_Cookie");

            entity.HasIndex(e => e.MemberID, "IX_CookiePurchase_Member");

            entity.Property(e => e.PurchaseID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookiePurchases)
                .HasForeignKey(d => d.CookieID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookiePurchase_Cookie");

            entity.HasOne(d => d.Member).WithMany(p => p.CookiePurchases)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookiePurchase_Member");
        });

        modelBuilder.Entity<CookieTagMap>(entity =>
        {
            entity.HasKey(e => new { e.CookieID, e.TagID });

            entity.ToTable("CookieTagMap");

            entity.Property(e => e.MappedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookieTagMaps)
                .HasForeignKey(d => d.CookieID)
                .HasConstraintName("FK_CookieTagMap_Cookie");

            entity.HasOne(d => d.Tag).WithMany(p => p.CookieTagMaps)
                .HasForeignKey(d => d.TagID)
                .HasConstraintName("FK_CookieTagMap_Tag");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.ItemID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ItemType).HasMaxLength(30);
            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.Rarity).HasMaxLength(16);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Member");

            entity.HasIndex(e => e.Email, "UQ_Member_Email").IsUnique();

            entity.HasIndex(e => e.EmailNormalized, "UQ_Member_EmailNormalized").IsUnique();

            entity.Property(e => e.MemberID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmailNormalized)
                .HasMaxLength(255)
                .HasComputedColumnSql("(lower([Email]))", true);
            entity.Property(e => e.LastLoginAt).HasPrecision(0);
            entity.Property(e => e.Nickname).HasMaxLength(80);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<MemberCat>(entity =>
        {
            entity.ToTable("MemberCat");

            entity.HasIndex(e => e.MemberID, "IX_MemberCat_Member");

            entity.HasIndex(e => new { e.MemberID, e.CatSpeciesID }, "UQ_MemberCat_Member_Species").IsUnique();

            entity.Property(e => e.MemberCatID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Nickname).HasMaxLength(80);
            entity.Property(e => e.Status).HasMaxLength(16);
            entity.Property(e => e.UnlockedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CatSpecies).WithMany(p => p.MemberCats)
                .HasForeignKey(d => d.CatSpeciesID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberCat_Species");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberCats)
                .HasForeignKey(d => d.MemberID)
                .HasConstraintName("FK_MemberCat_Member");
        });

        modelBuilder.Entity<MemberItem>(entity =>
        {
            entity.HasKey(e => new { e.MemberID, e.ItemID });

            entity.ToTable("MemberItem");

            entity.HasIndex(e => e.ItemID, "IX_MemberItem_Item");

            entity.HasIndex(e => e.MemberID, "IX_MemberItem_Member");

            entity.Property(e => e.OwnedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Item).WithMany(p => p.MemberItems)
                .HasForeignKey(d => d.ItemID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberItem_Item");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberItems)
                .HasForeignKey(d => d.MemberID)
                .HasConstraintName("FK_MemberItem_Member");
        });

        modelBuilder.Entity<MemberPoint>(entity =>
        {
            entity.HasKey(e => e.MemberID);

            entity.ToTable("MemberPoint");

            entity.Property(e => e.MemberID).ValueGeneratedNever();
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithOne(p => p.MemberPoint)
                .HasForeignKey<MemberPoint>(d => d.MemberID)
                .HasConstraintName("FK_MemberPoint_Member");
        });

        modelBuilder.Entity<MemberProfile>(entity =>
        {
            entity.HasKey(e => e.MemberID);

            entity.ToTable("MemberProfile");

            entity.HasIndex(e => e.AvatarID, "IX_MemberProfile_Avatar").HasFilter("([AvatarID] IS NOT NULL)");

            entity.Property(e => e.MemberID).ValueGeneratedNever();
            entity.Property(e => e.AvatarUpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Gender).HasMaxLength(20);

            entity.HasOne(d => d.Avatar).WithMany(p => p.MemberProfiles)
                .HasForeignKey(d => d.AvatarID)
                .HasConstraintName("FK_MemberProfile_Avatar");

            entity.HasOne(d => d.Member).WithOne(p => p.MemberProfile)
                .HasForeignKey<MemberProfile>(d => d.MemberID)
                .HasConstraintName("FK_MemberProfile_Member");
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.HasKey(e => e.TxnID);

            entity.ToTable("PointTransaction");

            entity.HasIndex(e => e.CreatedAt, "IX_PointTransaction_CreatedAt");

            entity.HasIndex(e => e.MemberID, "IX_PointTransaction_Member");

            entity.HasIndex(e => new { e.MemberID, e.CreatedAt }, "IX_PointTransaction_Member_Signed");

            entity.HasIndex(e => e.EventLogID, "UX_PointTransaction_EventLog")
                .IsUnique()
                .HasFilter("([EventLogID] IS NOT NULL)");

            entity.HasIndex(e => new { e.Source, e.RefID }, "UX_PointTransaction_SourceRef")
                .IsUnique()
                .HasFilter("([RefID] IS NOT NULL)");

            entity.Property(e => e.TxnID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Direction).HasMaxLength(8);
            entity.Property(e => e.SignedAmount).HasComputedColumnSql("(case when [Direction]=N'Earn' then [Amount] else  -[Amount] end)", true);
            entity.Property(e => e.Source).HasMaxLength(20);

            entity.HasOne(d => d.EventLog).WithOne(p => p.PointTransaction)
                .HasForeignKey<PointTransaction>(d => d.EventLogID)
                .HasConstraintName("FK_PointTransaction_EventLog");

            entity.HasOne(d => d.Member).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PointTransaction_Member");
        });

        modelBuilder.Entity<SetTagMap>(entity =>
        {
            entity.HasKey(e => new { e.SetID, e.TagID });

            entity.ToTable("SetTagMap");

            entity.Property(e => e.MappedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Set).WithMany(p => p.SetTagMaps)
                .HasForeignKey(d => d.SetID)
                .HasConstraintName("FK_SetTagMap_Set");

            entity.HasOne(d => d.Tag).WithMany(p => p.SetTagMaps)
                .HasForeignKey(d => d.TagID)
                .HasConstraintName("FK_SetTagMap_Tag");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tag");

            entity.HasIndex(e => e.Category, "IX_Tag_Category");

            entity.HasIndex(e => e.Name, "UQ_Tag_Name").IsUnique();

            entity.Property(e => e.TagID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasDefaultValue("一般");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<TimerTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateID);

            entity.ToTable("TimerTemplate");

            entity.Property(e => e.TemplateID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithMany(p => p.TimerTemplates)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimerTemplate_Member");
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.HasKey(e => e.SessionID);

            entity.ToTable("TrainingSession");

            entity.HasIndex(e => new { e.MemberID, e.StartedAt }, "IX_TrainingSession_Member_StartedAt").IsDescending(false, true);

            entity.HasIndex(e => e.SetID, "IX_TrainingSession_Set");

            entity.Property(e => e.SessionID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndedAt).HasPrecision(0);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.StartedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithMany(p => p.TrainingSessions)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSession_Member");

            entity.HasOne(d => d.Set).WithMany(p => p.TrainingSessions)
                .HasForeignKey(d => d.SetID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSession_Set");
        });

        modelBuilder.Entity<TrainingSessionItem>(entity =>
        {
            entity.HasKey(e => e.SessionItemID);

            entity.ToTable("TrainingSessionItem");

            entity.HasIndex(e => e.SessionID, "IX_TrainingSessionItem_Session");

            entity.HasIndex(e => new { e.SessionID, e.OrderNo }, "UQ_SessionItem_Session_Order").IsUnique();

            entity.Property(e => e.SessionItemID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualWeight).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasDefaultValue("Done");

            entity.HasOne(d => d.Session).WithMany(p => p.TrainingSessionItems)
                .HasForeignKey(d => d.SessionID)
                .HasConstraintName("FK_SessionItem_Session");

            entity.HasOne(d => d.SetItem).WithMany(p => p.TrainingSessionItems)
                .HasForeignKey(d => d.SetItemID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItem_SetItem");

            entity.HasOne(d => d.Video).WithMany(p => p.TrainingSessionItems)
                .HasForeignKey(d => d.VideoID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItem_Video");
        });

        modelBuilder.Entity<TrainingSet>(entity =>
        {
            entity.HasKey(e => e.SetID);

            entity.ToTable("TrainingSet");

            entity.Property(e => e.SetID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BodyPart)
                .HasMaxLength(50)
                .HasDefaultValue("全身");
            entity.Property(e => e.CoverUrl).HasMaxLength(512);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Difficulty).HasMaxLength(16);
            entity.Property(e => e.Equipment)
                .HasMaxLength(30)
                .HasDefaultValue("無器材");
            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.Status).HasMaxLength(16);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.OwnerMember).WithMany(p => p.TrainingSets)
                .HasForeignKey(d => d.OwnerMemberID)
                .HasConstraintName("FK_TrainingSet_Owner");
        });

        modelBuilder.Entity<TrainingSetItem>(entity =>
        {
            entity.HasKey(e => e.SetItemID);

            entity.ToTable("TrainingSetItem");

            entity.HasIndex(e => e.SetID, "IX_SetItem_Set");

            entity.HasIndex(e => new { e.SetID, e.OrderNo }, "UQ_TrainingSetItem_Set_Order").IsUnique();

            entity.Property(e => e.SetItemID).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Set).WithMany(p => p.TrainingSetItems)
                .HasForeignKey(d => d.SetID)
                .HasConstraintName("FK_TrainingSetItem_Set");

            entity.HasOne(d => d.Video).WithMany(p => p.TrainingSetItems)
                .HasForeignKey(d => d.VideoID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSetItem_Video");
        });

        modelBuilder.Entity<TrainingVideo>(entity =>
        {
            entity.HasKey(e => e.VideoID);

            entity.ToTable("TrainingVideo");

            entity.HasIndex(e => e.CreatedAt, "IX_TrainingVideo_CreatedAt").IsDescending();

            entity.Property(e => e.VideoID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BodyPart).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasMaxLength(16);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Url).HasMaxLength(500);
        });

        modelBuilder.Entity<VideoTagMap>(entity =>
        {
            entity.HasKey(e => new { e.VideoID, e.TagID });

            entity.ToTable("VideoTagMap");

            entity.Property(e => e.MappedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Tag).WithMany(p => p.VideoTagMaps)
                .HasForeignKey(d => d.TagID)
                .HasConstraintName("FK_VideoTagMap_Tag");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoTagMaps)
                .HasForeignKey(d => d.VideoID)
                .HasConstraintName("FK_VideoTagMap_Video");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
