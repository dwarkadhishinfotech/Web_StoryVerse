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
    }
}
