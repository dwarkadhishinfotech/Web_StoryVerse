using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Administrator", "Author", "Premium" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var authorEmail = "author@storyverse.com";
        var user = await userManager.FindByEmailAsync(authorEmail);
        
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                UserName = authorEmail, 
                Email = authorEmail,
                FirstName = "Author",
                LastName = "User",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Author");
            }
        }

        if (user != null)
        {
            // Check if user has any stories
            var existingStories = await context.Stories.Where(s => s.UserId == user.Id).ToListAsync();
            if (!existingStories.Any())
            {
                var story1 = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "The Shadows of Arcanis",
                    Genre = "Fantasy • Adventure",
                    CoverImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=300&auto=format&fit=crop&q=80",
                    Status = "InProgress",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                };

                var story2 = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Whispers of the Heart",
                    Genre = "Romance • Drama",
                    CoverImageUrl = "https://images.unsplash.com/photo-1516589178581-6cd7833ae3b2?w=300&auto=format&fit=crop&q=80",
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                };

                var story3 = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Echoes of Yesterday",
                    Genre = "Historical • Fiction",
                    CoverImageUrl = "https://images.unsplash.com/photo-1461360370896-922624d12aa1?w=300&auto=format&fit=crop&q=80",
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                };

                var story4 = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Beyond the Horizon",
                    Genre = "Sci-Fi • Thriller",
                    CoverImageUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=300&auto=format&fit=crop&q=80",
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                };

                context.Stories.AddRange(story1, story2, story3, story4);

                var char1 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story1.Id,
                    Name = "Alaric Vayne",
                    Role = "Protagonist",
                    ArcType = "Main",
                    Status = "Active",
                    OneLineDescription = "A loyal warrior torn between duty and destiny.",
                    AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                };

                var char2 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story1.Id,
                    Name = "Seraphina Lorne",
                    Role = "Mentor",
                    ArcType = "Supporting",
                    Status = "Active",
                    OneLineDescription = "A wise mentor with a mysterious past.",
                    AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                };

                var char3 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story1.Id,
                    Name = "Kael Draven",
                    Role = "Antagonist",
                    ArcType = "Main",
                    Status = "Active",
                    OneLineDescription = "A rival with ambition and a hidden agenda.",
                    AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                };

                var char4 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story2.Id,
                    Name = "Elara Moonwhisper",
                    Role = "Supporter",
                    ArcType = "Supporting",
                    Status = "Active",
                    OneLineDescription = "A healer with a deep connection to nature.",
                    AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                };

                var char5 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story3.Id,
                    Name = "Lord Marshall Eldric",
                    Role = "Authority",
                    ArcType = "Minor",
                    Status = "Draft",
                    OneLineDescription = "The ruling lord of Eldoria.",
                    AvatarUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                };

                var char6 = new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story4.Id,
                    Name = "Lira Dashwood",
                    Role = "Supporting",
                    ArcType = "Supporting",
                    Status = "Draft",
                    OneLineDescription = "The younger sister with big dreams.",
                    AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&auto=format&fit=crop&q=80",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                };

                context.Characters.AddRange(char1, char2, char3, char4, char5, char6);
                await context.SaveChangesAsync();
            }
        }

        await SeedWorldBuildingDefaultsAsync(context);
    }

    private static async Task SeedWorldBuildingDefaultsAsync(ApplicationDbContext context)
    {
        if (!await context.WorldEntityTypes.AnyAsync())
        {
            var types = new List<WorldEntityType>
            {
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Kingdom", Category = "Locations", Icon = "crown", Description = "A realm or country ruled by a king, queen, or sovereign.", DisplayOrder = 1, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Country", Category = "Locations", Icon = "flag", Description = "A nation with its own government, occupying a particular territory.", DisplayOrder = 2, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "City", Category = "Locations", Icon = "building-2", Description = "A major urban settlement with infrastructure and population.", DisplayOrder = 3, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Castle", Category = "Locations", Icon = "castle", Description = "A fortified stronghold or royal palace.", DisplayOrder = 4, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Village", Category = "Locations", Icon = "home", Description = "A small rural community or town.", DisplayOrder = 5, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Planet", Category = "Locations", Icon = "globe", Description = "A celestial body in a solar system.", DisplayOrder = 6, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Space Station", Category = "Locations", Icon = "orbit", Description = "An artificial orbital outpost.", DisplayOrder = 7, IsSystemDefault = true },

                new WorldEntityType { Id = Guid.NewGuid(), Name = "Military", Category = "Organizations", Icon = "shield", Description = "Armed forces or royal guard units.", DisplayOrder = 8, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Secret Society", Category = "Organizations", Icon = "eye", Description = "A hidden faction working in the shadows.", DisplayOrder = 9, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Guild", Category = "Organizations", Icon = "award", Description = "An association of craftsmen, mages, or merchants.", DisplayOrder = 10, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Police Department", Category = "Organizations", Icon = "badge", Description = "Law enforcement agency.", DisplayOrder = 11, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Crime Syndicate", Category = "Organizations", Icon = "skull", Description = "Underworld criminal syndicate or cartel.", DisplayOrder = 12, IsSystemDefault = true },

                new WorldEntityType { Id = Guid.NewGuid(), Name = "Faction", Category = "People Groups", Icon = "users", Description = "A political, ideological, or social group.", DisplayOrder = 13, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Dynasty", Category = "People Groups", Icon = "tree-pine", Description = "A hereditary line of rulers or noble family.", DisplayOrder = 14, IsSystemDefault = true },

                new WorldEntityType { Id = Guid.NewGuid(), Name = "Faith", Category = "Cultures", Icon = "sparkles", Description = "A religious belief system or pantheon.", DisplayOrder = 15, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Religion", Category = "Religions", Icon = "church", Description = "Religious organization, faith, or belief system.", DisplayOrder = 16, IsSystemDefault = true },

                new WorldEntityType { Id = Guid.NewGuid(), Name = "Species", Category = "Species", Icon = "paw-print", Description = "Biological or magical species.", DisplayOrder = 17, IsSystemDefault = true },
                new WorldEntityType { Id = Guid.NewGuid(), Name = "Alien Species", Category = "Species", Icon = "bot", Description = "Extraterrestrial lifeform.", DisplayOrder = 18, IsSystemDefault = true },

                new WorldEntityType { Id = Guid.NewGuid(), Name = "Historical Event", Category = "Historical Events", Icon = "history", Description = "Significant past war, treaty, or occurrence.", DisplayOrder = 19, IsSystemDefault = true }
            };

            context.WorldEntityTypes.AddRange(types);
            await context.SaveChangesAsync();
        }
        else
        {
            if (!await context.WorldEntityTypes.AnyAsync(t => t.Name == "Country"))
            {
                context.WorldEntityTypes.Add(new WorldEntityType
                {
                    Id = Guid.NewGuid(),
                    Name = "Country",
                    Category = "Locations",
                    Icon = "flag",
                    Description = "A nation with its own government, occupying a particular territory.",
                    DisplayOrder = 2,
                    IsSystemDefault = true
                });
                await context.SaveChangesAsync();
            }
        }

        if (!await context.WorldTemplates.AnyAsync())
        {
            var templates = new List<WorldTemplate>
            {
                new WorldTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Fantasy Kingdom",
                    Genre = "Fantasy",
                    Icon = "castle",
                    SubTypesSummary = "Kingdom, City, Castle, Village, Guild...",
                    Description = "Comprehensive template for high fantasy worlds with noble dynasties, magic guilds, and sacred temples."
                },
                new WorldTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Modern City",
                    Genre = "Contemporary",
                    Icon = "building-2",
                    SubTypesSummary = "City, District, Building, Landmark...",
                    Description = "Urban template for contemporary thrillers, romances, and mystery stories."
                },
                new WorldTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Crime World",
                    Genre = "Crime Thriller",
                    Icon = "scale",
                    SubTypesSummary = "Police Station, Crime Syndicate, Court...",
                    Description = "Dark crime thriller template covering police precincts, underworld syndicates, and courtrooms."
                },
                new WorldTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Sci-Fi Universe",
                    Genre = "Sci-Fi",
                    Icon = "orbit",
                    SubTypesSummary = "Planet, Space Station, Faction...",
                    Description = "Interstellar template for futuristic planets, orbital stations, alien species, and hyper-tech."
                },
                new WorldTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Historical Realm",
                    Genre = "Historical",
                    Icon = "landmark",
                    SubTypesSummary = "Empire, Dynasty, Military, Trade Route...",
                    Description = "Historical fiction template for ancient empires, military campaigns, and feudal dynasties."
                }
            };

            context.WorldTemplates.AddRange(templates);
            await context.SaveChangesAsync();
        }
    }
}
