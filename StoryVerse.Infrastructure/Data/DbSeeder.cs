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

    public static async Task SeedTimelineDataForStoryAsync(ApplicationDbContext context, Guid storyId)
    {
        var story = await context.Stories.FindAsync(storyId);
        if (story == null) return;

        // Check if story already has timeline events
        if (await context.TimelineEvents.AnyAsync(e => e.StoryId == storyId))
        {
            return;
        }

        // Seed Characters if none exist for this story
        var existingCharacters = await context.Characters.Where(c => c.StoryId == storyId).ToListAsync();
        if (!existingCharacters.Any())
        {
            var char1 = new Character { Id = Guid.NewGuid(), StoryId = storyId, Name = "Sameer Malhotra", Role = "Protagonist", Status = "Active", OneLineDescription = "Sub-Inspector in Silverbrook Crime Branch." };
            var char2 = new Character { Id = Guid.NewGuid(), StoryId = storyId, Name = "Riya", Role = "Protagonist", Status = "Active", OneLineDescription = "Investigative researcher at Silverbrook." };
            var char3 = new Character { Id = Guid.NewGuid(), StoryId = storyId, Name = "Raj Malhotra", Role = "Supporting", Status = "Active", OneLineDescription = "Father of Sameer." };
            var char4 = new Character { Id = Guid.NewGuid(), StoryId = storyId, Name = "Meera Malhotra", Role = "Supporting", Status = "Active", OneLineDescription = "Mother of Sameer." };
            var char5 = new Character { Id = Guid.NewGuid(), StoryId = storyId, Name = "Inspector Sharma", Role = "Secondary", Status = "Active", OneLineDescription = "Senior officer at Crime Branch." };

            context.Characters.AddRange(char1, char2, char3, char4, char5);
            await context.SaveChangesAsync();
            existingCharacters = new List<Character> { char1, char2, char3, char4, char5 };
        }

        var sameer = existingCharacters.FirstOrDefault(c => c.Name.Contains("Sameer")) ?? existingCharacters[0];
        var riya = existingCharacters.FirstOrDefault(c => c.Name.Contains("Riya")) ?? existingCharacters.ElementAtOrDefault(1) ?? sameer;
        var raj = existingCharacters.FirstOrDefault(c => c.Name.Contains("Raj")) ?? sameer;
        var meera = existingCharacters.FirstOrDefault(c => c.Name.Contains("Meera")) ?? sameer;
        var sharma = existingCharacters.FirstOrDefault(c => c.Name.Contains("Sharma")) ?? sameer;

        // Seed Story Arcs
        var arc1 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Main Story", ArcType = "Main Story", Color = "#0D9488", TargetCompletionPercent = 72, DisplayOrder = 1 };
        var arc2 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Romance Arc", ArcType = "Romance Arc", Color = "#F43F5E", TargetCompletionPercent = 48, DisplayOrder = 2 };
        var arc3 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Mystery Arc", ArcType = "Mystery Arc", Color = "#8B5CF6", TargetCompletionPercent = 63, DisplayOrder = 3 };
        var arc4 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Crime Investigation", ArcType = "Crime Investigation", Color = "#F59E0B", TargetCompletionPercent = 55, DisplayOrder = 4 };
        var arc5 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Past Trauma Arc", ArcType = "Past Trauma Arc", Color = "#3B82F6", TargetCompletionPercent = 38, DisplayOrder = 5 };
        var arc6 = new StoryArc { Id = Guid.NewGuid(), StoryId = storyId, Title = "Redemption Arc", ArcType = "Redemption Arc", Color = "#10B981", TargetCompletionPercent = 20, DisplayOrder = 6 };

        context.StoryArcs.AddRange(arc1, arc2, arc3, arc4, arc5, arc6);

        // Seed Story Timeline metadata
        var storyTimeline = new StoryTimeline
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Name = "Revenge for Love Master Timeline",
            TimelineType = "Chronological Timeline",
            Color = "Teal",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.StoryTimelines.Add(storyTimeline);

        // Seed Events
        var e1 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Sameer Malhotra is Born",
            Category = "Birth",
            EventType = "Backstory",
            StoryDate = "12 Jan 2020",
            RealDate = new DateTime(2020, 1, 12),
            LocationName = "Silverbrook City, Eldoria",
            Summary = "Sameer was born to Raj Malhotra and Meera Malhotra in Silverbrook City.",
            Color = "#10B981",
            Icon = "baby",
            Importance = "Medium",
            DisplayOrder = 1
        };
        e1.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e1.Id, CharacterId = sameer.Id, Role = "Protagonist" });
        e1.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e1.Id, CharacterId = raj.Id, Role = "Participant" });
        e1.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e1.Id, CharacterId = meera.Id, Role = "Participant" });

        var e2 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Sameer Meets Riya",
            Category = "Meeting",
            EventType = "Standard",
            StoryDate = "14 Feb 2020",
            RealDate = new DateTime(2020, 2, 14),
            LocationName = "Silverbrook University",
            Summary = "A chance meeting in the library changes everything.",
            Color = "#F43F5E",
            Icon = "heart",
            Importance = "High",
            DisplayOrder = 2
        };
        e2.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e2.Id, CharacterId = sameer.Id, Role = "Protagonist" });
        e2.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e2.Id, CharacterId = riya.Id, Role = "Protagonist" });

        var e3 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Sameer Joins Crime Branch",
            Category = "Career",
            EventType = "Milestone",
            StoryDate = "03 Mar 2020",
            RealDate = new DateTime(2020, 3, 3),
            LocationName = "Crime Branch Headquarters, Silverbrook",
            Summary = "Sameer officially joins the Crime Branch as a Sub-Inspector.",
            Color = "#F59E0B",
            Icon = "shield",
            Importance = "High",
            DisplayOrder = 3
        };
        e3.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e3.Id, CharacterId = sameer.Id, Role = "Protagonist" });
        e3.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e3.Id, CharacterId = sharma.Id, Role = "Leader" });
        e3.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e3.Id, CharacterId = raj.Id, Role = "Participant" });

        var e4 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "First Major Case",
            Category = "Investigation",
            EventType = "Standard",
            StoryDate = "21 Apr 2020",
            RealDate = new DateTime(2020, 4, 21),
            LocationName = "Dockside Area, Silverbrook",
            Summary = "A mysterious murder shakes the city. Sameer leads the investigation.",
            Color = "#8B5CF6",
            Icon = "crosshair",
            Importance = "Critical",
            DisplayOrder = 4
        };
        e4.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e4.Id, CharacterId = sameer.Id, Role = "Detective" });
        e4.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e4.Id, CharacterId = sharma.Id, Role = "Participant" });

        var e5 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Bomb Blast in Old Market",
            Category = "Incident",
            EventType = "Climax",
            StoryDate = "10 Jun 2020",
            RealDate = new DateTime(2020, 6, 10),
            LocationName = "Old Market, Silverbrook",
            Summary = "A powerful blast injures many. The case takes a dark turn.",
            Color = "#F97316",
            Icon = "sun",
            Importance = "Critical",
            DisplayOrder = 5
        };
        e5.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e5.Id, CharacterId = sameer.Id, Role = "Detective" });
        e5.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e5.Id, CharacterId = riya.Id, Role = "Witness" });
        e5.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e5.Id, CharacterId = meera.Id, Role = "Victim" });

        var e6 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Court Hearing",
            Category = "Investigation",
            EventType = "Milestone",
            StoryDate = "21 May 2025 10:00 AM",
            RealDate = new DateTime(2025, 5, 21, 10, 0, 0),
            LocationName = "High Court of Silverbrook",
            Summary = "The preliminary trial begins against the prime suspect.",
            Color = "#3B82F6",
            Icon = "scale",
            Importance = "High",
            DisplayOrder = 6
        };
        e6.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e6.Id, CharacterId = sameer.Id, Role = "Participant" });
        e6.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e6.Id, CharacterId = riya.Id, Role = "Participant" });

        var e7 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Confrontation",
            Category = "Incident",
            EventType = "Standard",
            StoryDate = "23 May 2025 04:00 PM",
            RealDate = new DateTime(2025, 5, 23, 16, 0, 0),
            LocationName = "Dockside Area, Silverbrook",
            Summary = "A tense face-off at the docks reveals the mastermind's identity.",
            Color = "#EF4444",
            Icon = "alert-triangle",
            Importance = "Critical",
            DisplayOrder = 7
        };
        e7.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e7.Id, CharacterId = sameer.Id, Role = "Protagonist" });
        e7.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e7.Id, CharacterId = riya.Id, Role = "Participant" });

        var e8 = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Riya's Decision",
            Category = "Meeting",
            EventType = "Climax",
            StoryDate = "25 May 2025 09:00 AM",
            RealDate = new DateTime(2025, 5, 25, 9, 0, 0),
            LocationName = "Silverbrook University",
            Summary = "Riya makes a choice that will alter the course of the investigation.",
            Color = "#EC4899",
            Icon = "heart",
            Importance = "High",
            DisplayOrder = 8
        };
        e8.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e8.Id, CharacterId = riya.Id, Role = "Protagonist" });
        e8.CharacterLinks.Add(new TimelineCharacter { Id = Guid.NewGuid(), TimelineEventId = e8.Id, CharacterId = sameer.Id, Role = "Participant" });

        context.TimelineEvents.AddRange(e1, e2, e3, e4, e5, e6, e7, e8);
        await context.SaveChangesAsync();
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
