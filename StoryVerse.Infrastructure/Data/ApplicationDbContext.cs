using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;

namespace StoryVerse.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Story> Stories { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<DropdownOption> DropdownOptions { get; set; }

    public DbSet<WorldEntityType> WorldEntityTypes { get; set; }
    public DbSet<WorldEntityField> WorldEntityFields { get; set; }
    public DbSet<WorldEntity> WorldEntities { get; set; }
    public DbSet<WorldEntityValue> WorldEntityValues { get; set; }
    public DbSet<WorldEntityRelationship> WorldEntityRelationships { get; set; }
    public DbSet<WorldEntityCharacter> WorldEntityCharacters { get; set; }
    public DbSet<WorldEntityTimeline> WorldEntityTimelines { get; set; }
    public DbSet<WorldTemplate> WorldTemplates { get; set; }
    public DbSet<WorldMap> WorldMaps { get; set; }
    public DbSet<WorldMapMarker> WorldMapMarkers { get; set; }

    // Timeline Module
    public DbSet<TimelineEvent> TimelineEvents { get; set; }
    public DbSet<TimelineCharacter> TimelineCharacters { get; set; }
    public DbSet<TimelineWorldEntity> TimelineWorldEntities { get; set; }
    public DbSet<TimelineRelationship> TimelineRelationships { get; set; }
    public DbSet<TimelineEventChapter> TimelineEventChapters { get; set; }
    public DbSet<StoryArc> StoryArcs { get; set; }
    public DbSet<StoryArcEvent> StoryArcEvents { get; set; }
    public DbSet<StoryTimeline> StoryTimelines { get; set; }
    public DbSet<TimelineStoryArc> TimelineStoryArcs { get; set; }

    // Domain Relationships
    public DbSet<CharacterRelationship> CharacterRelationships { get; set; }
    public DbSet<CharacterWorldEntity> CharacterWorldEntities { get; set; }
    public DbSet<ChapterCharacter> ChapterCharacters { get; set; }
    public DbSet<ChapterWorldEntity> ChapterWorldEntities { get; set; }

    // Research & Assets
    public DbSet<ResearchNote> ResearchNotes { get; set; }
    public DbSet<ResearchCharacter> ResearchCharacters { get; set; }
    public DbSet<ResearchWorldEntity> ResearchWorldEntities { get; set; }
    public DbSet<ResearchTimelineEvent> ResearchTimelineEvents { get; set; }
    public DbSet<ResearchChapter> ResearchChapters { get; set; }
    public DbSet<ResearchAsset> ResearchAssets { get; set; }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetCharacter> AssetCharacters { get; set; }
    public DbSet<AssetWorldEntity> AssetWorldEntities { get; set; }
    public DbSet<AssetTimelineEvent> AssetTimelineEvents { get; set; }
    public DbSet<AssetChapter> AssetChapters { get; set; }
    public DbSet<AssetResearchNote> AssetResearchNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Customize the ASP.NET Identity model and override the defaults if needed.
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("DI_MST_AspNetUsers");
        });
        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("DI_MST_AspNetRoles");
        });
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("DI_TRN_AspNetUserRoles");
        });
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("DI_TRN_AspNetUserClaims");
        });
        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("DI_TRN_AspNetUserLogins");
        });
        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("DI_TRN_AspNetUserTokens");
        });
        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("DI_TRN_AspNetRoleClaims");
        });

        builder.Entity<Story>(entity =>
        {
            entity.ToTable("DI_TRN_WebStories");
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Chapter>(entity =>
        {
            entity.ToTable("DI_TRN_WebChapters");
            entity.HasOne(c => c.Story)
                .WithMany(s => s.Chapters)
                .HasForeignKey(c => c.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Character>(entity =>
        {
            entity.ToTable("DI_TRN_WebCharacters");
            entity.HasOne(c => c.Story)
                .WithMany(s => s.Characters)
                .HasForeignKey(c => c.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Location>(entity =>
        {
            entity.ToTable("DI_MST_WebLocations");
            entity.HasOne(l => l.Story)
                .WithMany(s => s.Locations)
                .HasForeignKey(l => l.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("DI_TRN_ActivityLogs");
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserGoal>(entity =>
        {
            entity.ToTable("DI_TRN_UserGoals");
            entity.HasKey(ug => ug.UserId);
            entity.HasOne(ug => ug.User)
                .WithOne()
                .HasForeignKey<UserGoal>(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DropdownOption>(entity =>
        {
            entity.ToTable("DI_MST_DropdownOptions");
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.Category);
        });

        builder.Entity<WorldEntityType>(entity =>
        {
            entity.ToTable("DI_MST_WorldEntityTypes");
            entity.HasKey(e => e.Id);
        });

        builder.Entity<WorldEntityField>(entity =>
        {
            entity.ToTable("DI_MST_WorldEntityFields");
            entity.HasKey(f => f.Id);
            entity.HasOne(f => f.EntityType)
                .WithMany(t => t.Fields)
                .HasForeignKey(f => f.EntityTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_WorldEntities");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Story)
                .WithMany()
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.EntityType)
                .WithMany(t => t.Entities)
                .HasForeignKey(e => e.EntityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentEntity)
                .WithMany(e => e.SubEntities)
                .HasForeignKey(e => e.ParentEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorldEntityValue>(entity =>
        {
            entity.ToTable("DI_TRN_WorldEntityValues");
            entity.HasKey(v => v.Id);
            entity.HasOne(v => v.Entity)
                .WithMany(e => e.FieldValues)
                .HasForeignKey(v => v.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Field)
                .WithMany(f => f.FieldValues)
                .HasForeignKey(v => v.FieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorldEntityRelationship>(entity =>
        {
            entity.ToTable("DI_TRN_WorldEntityRelationships");
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.SourceEntity)
                .WithMany(e => e.SourceRelationships)
                .HasForeignKey(r => r.SourceEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.TargetEntity)
                .WithMany(e => e.TargetRelationships)
                .HasForeignKey(r => r.TargetEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorldEntityCharacter>(entity =>
        {
            entity.ToTable("DI_TRN_WorldEntityCharacters");
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Entity)
                .WithMany(e => e.CharacterLinks)
                .HasForeignKey(c => c.EntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Character)
                .WithMany()
                .HasForeignKey(c => c.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorldEntityTimeline>(entity =>
        {
            entity.ToTable("DI_TRN_WorldEntityTimelines");
            entity.HasKey(t => t.Id);

            entity.HasOne(t => t.Entity)
                .WithMany(e => e.TimelineLinks)
                .HasForeignKey(t => t.EntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorldTemplate>(entity =>
        {
            entity.ToTable("DI_MST_WorldTemplates");
            entity.HasKey(t => t.Id);
        });

        builder.Entity<WorldMap>(entity =>
        {
            entity.ToTable("DI_TRN_WorldMaps");
            entity.HasKey(m => m.Id);
            entity.HasOne(m => m.Story)
                .WithMany()
                .HasForeignKey(m => m.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorldMapMarker>(entity =>
        {
            entity.ToTable("DI_TRN_WorldMapMarkers");
            entity.HasKey(m => m.Id);
            entity.HasOne(m => m.Map)
                .WithMany(map => map.Markers)
                .HasForeignKey(m => m.MapId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Entity)
                .WithMany()
                .HasForeignKey(m => m.EntityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // --- TIMELINE MODULE CONFIGURATIONS ---
        builder.Entity<TimelineEvent>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineEvents");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Story)
                .WithMany(s => s.TimelineEvents)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TimelineCharacter>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineCharacters");
            entity.HasKey(tc => tc.Id);
            entity.HasOne(tc => tc.TimelineEvent)
                .WithMany(e => e.CharacterLinks)
                .HasForeignKey(tc => tc.TimelineEventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tc => tc.Character)
                .WithMany()
                .HasForeignKey(tc => tc.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TimelineWorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineWorldEntities");
            entity.HasKey(tw => tw.Id);
            entity.HasOne(tw => tw.TimelineEvent)
                .WithMany(e => e.WorldEntityLinks)
                .HasForeignKey(tw => tw.TimelineEventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tw => tw.WorldEntity)
                .WithMany()
                .HasForeignKey(tw => tw.WorldEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TimelineRelationship>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineRelationships");
            entity.HasKey(tr => tr.Id);
            entity.HasOne(tr => tr.SourceEvent)
                .WithMany(e => e.SourceRelationships)
                .HasForeignKey(tr => tr.SourceEventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tr => tr.TargetEvent)
                .WithMany(e => e.TargetRelationships)
                .HasForeignKey(tr => tr.TargetEventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TimelineEventChapter>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineEventChapters");
            entity.HasKey(tc => tc.Id);
            entity.HasOne(tc => tc.TimelineEvent)
                .WithMany(e => e.ChapterLinks)
                .HasForeignKey(tc => tc.TimelineEventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tc => tc.Chapter)
                .WithMany()
                .HasForeignKey(tc => tc.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StoryArc>(entity =>
        {
            entity.ToTable("DI_TRN_StoryArcs");
            entity.HasKey(sa => sa.Id);
            entity.HasOne(sa => sa.Story)
                .WithMany(s => s.StoryArcs)
                .HasForeignKey(sa => sa.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StoryArcEvent>(entity =>
        {
            entity.ToTable("DI_TRN_StoryArcEvents");
            entity.HasKey(sae => sae.Id);
            entity.HasOne(sae => sae.StoryArc)
                .WithMany(sa => sa.ArcEvents)
                .HasForeignKey(sae => sae.StoryArcId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(sae => sae.TimelineEvent)
                .WithMany(e => e.ArcLinks)
                .HasForeignKey(sae => sae.TimelineEventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StoryTimeline>(entity =>
        {
            entity.ToTable("DI_TRN_StoryTimelines");
            entity.HasKey(st => st.Id);
            entity.HasOne(st => st.Story)
                .WithMany(s => s.StoryTimelines)
                .HasForeignKey(st => st.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TimelineStoryArc>(entity =>
        {
            entity.ToTable("DI_TRN_TimelineStoryArcs");
            entity.HasKey(tsa => tsa.Id);
            entity.HasOne(tsa => tsa.StoryTimeline)
                .WithMany(st => st.LinkedStoryArcs)
                .HasForeignKey(tsa => tsa.StoryTimelineId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tsa => tsa.StoryArc)
                .WithMany()
                .HasForeignKey(tsa => tsa.StoryArcId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- CHARACTER & DOMAIN RELATIONSHIPS ---
        builder.Entity<CharacterRelationship>(entity =>
        {
            entity.ToTable("DI_TRN_CharacterRelationships");
            entity.HasKey(cr => cr.Id);
            entity.HasOne(cr => cr.Story)
                .WithMany()
                .HasForeignKey(cr => cr.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cr => cr.SourceCharacter)
                .WithMany()
                .HasForeignKey(cr => cr.SourceCharacterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(cr => cr.TargetCharacter)
                .WithMany()
                .HasForeignKey(cr => cr.TargetCharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CharacterWorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_CharacterWorldEntities");
            entity.HasKey(cw => cw.Id);
            entity.HasOne(cw => cw.Character)
                .WithMany()
                .HasForeignKey(cw => cw.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cw => cw.WorldEntity)
                .WithMany()
                .HasForeignKey(cw => cw.WorldEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ChapterCharacter>(entity =>
        {
            entity.ToTable("DI_TRN_ChapterCharacters");
            entity.HasKey(cc => cc.Id);
            entity.HasOne(cc => cc.Chapter)
                .WithMany()
                .HasForeignKey(cc => cc.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cc => cc.Character)
                .WithMany()
                .HasForeignKey(cc => cc.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ChapterWorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_ChapterWorldEntities");
            entity.HasKey(cw => cw.Id);
            entity.HasOne(cw => cw.Chapter)
                .WithMany()
                .HasForeignKey(cw => cw.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cw => cw.WorldEntity)
                .WithMany()
                .HasForeignKey(cw => cw.WorldEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- RESEARCH NOTES ---
        builder.Entity<ResearchNote>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchNotes");
            entity.HasKey(rn => rn.Id);
            entity.HasOne(rn => rn.Story)
                .WithMany(s => s.ResearchNotes)
                .HasForeignKey(rn => rn.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ResearchCharacter>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchCharacters");
            entity.HasKey(rc => rc.Id);
            entity.HasOne(rc => rc.ResearchNote)
                .WithMany(rn => rn.CharacterLinks)
                .HasForeignKey(rc => rc.ResearchNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rc => rc.Character)
                .WithMany()
                .HasForeignKey(rc => rc.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ResearchWorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchWorldEntities");
            entity.HasKey(rw => rw.Id);
            entity.HasOne(rw => rw.ResearchNote)
                .WithMany(rn => rn.WorldEntityLinks)
                .HasForeignKey(rw => rw.ResearchNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rw => rw.WorldEntity)
                .WithMany()
                .HasForeignKey(rw => rw.WorldEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ResearchTimelineEvent>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchTimelineEvents");
            entity.HasKey(rt => rt.Id);
            entity.HasOne(rt => rt.ResearchNote)
                .WithMany(rn => rn.TimelineEventLinks)
                .HasForeignKey(rt => rt.ResearchNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rt => rt.TimelineEvent)
                .WithMany(e => e.ResearchLinks)
                .HasForeignKey(rt => rt.TimelineEventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ResearchChapter>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchChapters");
            entity.HasKey(rc => rc.Id);
            entity.HasOne(rc => rc.ResearchNote)
                .WithMany(rn => rn.ChapterLinks)
                .HasForeignKey(rc => rc.ResearchNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rc => rc.Chapter)
                .WithMany()
                .HasForeignKey(rc => rc.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ResearchAsset>(entity =>
        {
            entity.ToTable("DI_TRN_ResearchAssets");
            entity.HasKey(ra => ra.Id);
            entity.HasOne(ra => ra.ResearchNote)
                .WithMany(rn => rn.AssetLinks)
                .HasForeignKey(ra => ra.ResearchNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ra => ra.Asset)
                .WithMany()
                .HasForeignKey(ra => ra.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- ASSETS ---
        builder.Entity<Asset>(entity =>
        {
            entity.ToTable("DI_TRN_Assets");
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Story)
                .WithMany(s => s.Assets)
                .HasForeignKey(a => a.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssetCharacter>(entity =>
        {
            entity.ToTable("DI_TRN_AssetCharacters");
            entity.HasKey(ac => ac.Id);
            entity.HasOne(ac => ac.Asset)
                .WithMany(a => a.CharacterLinks)
                .HasForeignKey(ac => ac.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ac => ac.Character)
                .WithMany()
                .HasForeignKey(ac => ac.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssetWorldEntity>(entity =>
        {
            entity.ToTable("DI_TRN_AssetWorldEntities");
            entity.HasKey(aw => aw.Id);
            entity.HasOne(aw => aw.Asset)
                .WithMany(a => a.WorldEntityLinks)
                .HasForeignKey(aw => aw.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(aw => aw.WorldEntity)
                .WithMany()
                .HasForeignKey(aw => aw.WorldEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssetTimelineEvent>(entity =>
        {
            entity.ToTable("DI_TRN_AssetTimelineEvents");
            entity.HasKey(at => at.Id);
            entity.HasOne(at => at.Asset)
                .WithMany(a => a.TimelineEventLinks)
                .HasForeignKey(at => at.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(at => at.TimelineEvent)
                .WithMany(e => e.AssetLinks)
                .HasForeignKey(at => at.TimelineEventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssetChapter>(entity =>
        {
            entity.ToTable("DI_TRN_AssetChapters");
            entity.HasKey(ac => ac.Id);
            entity.HasOne(ac => ac.Asset)
                .WithMany(a => a.ChapterLinks)
                .HasForeignKey(ac => ac.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ac => ac.Chapter)
                .WithMany()
                .HasForeignKey(ac => ac.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssetResearchNote>(entity =>
        {
            entity.ToTable("DI_TRN_AssetResearchNotes");
            entity.HasKey(ar => ar.Id);
            entity.HasOne(ar => ar.Asset)
                .WithMany(a => a.ResearchLinks)
                .HasForeignKey(ar => ar.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ar => ar.ResearchNote)
                .WithMany()
                .HasForeignKey(ar => ar.ResearchNoteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
