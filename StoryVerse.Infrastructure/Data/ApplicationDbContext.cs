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
    }
}
