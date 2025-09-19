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
            entity.Property(e => e.AvatarID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CatEvent>(entity =>
        {
            entity.Property(e => e.EventID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CatEventLog>(entity =>
        {
            entity.HasIndex(e => e.FeedID, "UX_CatEventLog_FeedID")
                .IsUnique()
                .HasFilter("([FeedID] IS NOT NULL)");

            entity.Property(e => e.LogID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Event).WithMany(p => p.CatEventLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CatEventLog_Event");

            entity.HasOne(d => d.Feed).WithOne(p => p.CatEventLog).HasConstraintName("FK_CatEventLog_Feed");

            entity.HasOne(d => d.MemberCat).WithMany(p => p.CatEventLogs).HasConstraintName("FK_CatEventLog_MemberCat");
        });

        modelBuilder.Entity<CatPattern>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CatSpecy>(entity =>
        {
            entity.Property(e => e.CatSpeciesID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.PatternCodeNavigation).WithMany(p => p.CatSpecies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CatSpecies_Pattern");
        });

        modelBuilder.Entity<Cookie>(entity =>
        {
            entity.Property(e => e.CookieID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CookieFeedLog>(entity =>
        {
            entity.HasIndex(e => e.EventLogID, "UX_CookieFeedLog_EventLog")
                .IsUnique()
                .HasFilter("([EventLogID] IS NOT NULL)");

            entity.Property(e => e.FeedID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookieFeedLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_Cookie");

            entity.HasOne(d => d.EventLog).WithOne(p => p.CookieFeedLog).HasConstraintName("FK_CookieFeedLog_EventLog");

            entity.HasOne(d => d.MemberCat).WithMany(p => p.CookieFeedLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_MemberCat");

            entity.HasOne(d => d.Member).WithMany(p => p.CookieFeedLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookieFeedLog_Member");
        });

        modelBuilder.Entity<CookiePurchase>(entity =>
        {
            entity.Property(e => e.PurchaseID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookiePurchases)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookiePurchase_Cookie");

            entity.HasOne(d => d.Member).WithMany(p => p.CookiePurchases)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CookiePurchase_Member");
        });

        modelBuilder.Entity<CookieTagMap>(entity =>
        {
            entity.Property(e => e.MappedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cookie).WithMany(p => p.CookieTagMaps).HasConstraintName("FK_CookieTagMap_Cookie");

            entity.HasOne(d => d.Tag).WithMany(p => p.CookieTagMaps).HasConstraintName("FK_CookieTagMap_Tag");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.ItemID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.Property(e => e.MemberID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EmailNormalized).HasComputedColumnSql("(lower([Email]))", true);
            entity.Property(e => e.Status).HasDefaultValue("Active");
        });

        modelBuilder.Entity<MemberCat>(entity =>
        {
            entity.Property(e => e.MemberCatID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.UnlockedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CatSpecies).WithMany(p => p.MemberCats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberCat_Species");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberCats).HasConstraintName("FK_MemberCat_Member");
        });

        modelBuilder.Entity<MemberItem>(entity =>
        {
            entity.Property(e => e.OwnedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Item).WithMany(p => p.MemberItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberItem_Item");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberItems).HasConstraintName("FK_MemberItem_Member");
        });

        modelBuilder.Entity<MemberPoint>(entity =>
        {
            entity.Property(e => e.MemberID).ValueGeneratedNever();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithOne(p => p.MemberPoint).HasConstraintName("FK_MemberPoint_Member");
        });

        modelBuilder.Entity<MemberProfile>(entity =>
        {
            entity.HasIndex(e => e.AvatarID, "IX_MemberProfile_Avatar").HasFilter("([AvatarID] IS NOT NULL)");

            entity.Property(e => e.MemberID).ValueGeneratedNever();
            entity.Property(e => e.AvatarUpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Avatar).WithMany(p => p.MemberProfiles).HasConstraintName("FK_MemberProfile_Avatar");

            entity.HasOne(d => d.Member).WithOne(p => p.MemberProfile).HasConstraintName("FK_MemberProfile_Member");
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.HasIndex(e => e.EventLogID, "UX_PointTransaction_EventLog")
                .IsUnique()
                .HasFilter("([EventLogID] IS NOT NULL)");

            entity.HasIndex(e => new { e.Source, e.RefID }, "UX_PointTransaction_SourceRef")
                .IsUnique()
                .HasFilter("([RefID] IS NOT NULL)");

            entity.Property(e => e.TxnID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SignedAmount).HasComputedColumnSql("(case when [Direction]=N'Earn' then [Amount] else  -[Amount] end)", true);

            entity.HasOne(d => d.EventLog).WithOne(p => p.PointTransaction).HasConstraintName("FK_PointTransaction_EventLog");

            entity.HasOne(d => d.Member).WithMany(p => p.PointTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PointTransaction_Member");
        });

        modelBuilder.Entity<SetTagMap>(entity =>
        {
            entity.Property(e => e.MappedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Set).WithMany(p => p.SetTagMaps).HasConstraintName("FK_SetTagMap_Set");

            entity.HasOne(d => d.Tag).WithMany(p => p.SetTagMaps).HasConstraintName("FK_SetTagMap_Tag");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(e => e.TagID).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Category)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("一般");

            entity.HasIndex(e => e.Category)
                  .HasDatabaseName("IX_Tag_Category");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Tag_Category", "[Category] IN (N'部位', N'一般')");
            });

        });


        modelBuilder.Entity<TimerTemplate>(entity =>
        {
            entity.Property(e => e.TemplateID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithMany(p => p.TimerTemplates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimerTemplate_Member");
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.Property(e => e.SessionID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Member).WithMany(p => p.TrainingSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSession_Member");

            entity.HasOne(d => d.Set).WithMany(p => p.TrainingSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSession_Set");
        });

        modelBuilder.Entity<TrainingSessionItem>(entity =>
        {
            entity.Property(e => e.SessionItemID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Status).HasDefaultValue("Done");

            entity.HasOne(d => d.Session).WithMany(p => p.TrainingSessionItems).HasConstraintName("FK_SessionItem_Session");

            entity.HasOne(d => d.SetItem).WithMany(p => p.TrainingSessionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItem_SetItem");

            entity.HasOne(d => d.Video).WithMany(p => p.TrainingSessionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItem_Video");
        });

        modelBuilder.Entity<TrainingSet>(entity =>
        {
            entity.Property(e => e.SetId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BodyPart).HasDefaultValue("全身");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Equipment).HasDefaultValue("無器材");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.OwnerMember).WithMany(p => p.TrainingSets).HasConstraintName("FK_TrainingSet_Owner");
        });

        modelBuilder.Entity<TrainingSetItem>(entity =>
        {
            entity.Property(e => e.SetItemId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Set).WithMany(p => p.TrainingSetItems).HasConstraintName("FK_TrainingSetItem_Set");

            entity.HasOne(d => d.Video).WithMany(p => p.TrainingSetItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingSetItem_Video");
        });

        modelBuilder.Entity<TrainingVideo>(entity =>
        {
            entity.Property(e => e.VideoId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<VideoTagMap>(entity =>
        {
            entity.Property(e => e.MappedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Tag).WithMany(p => p.VideoTagMaps).HasConstraintName("FK_VideoTagMap_Tag");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoTagMaps).HasConstraintName("FK_VideoTagMap_Video");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
