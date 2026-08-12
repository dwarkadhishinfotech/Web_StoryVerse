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
        await EnsureDatabaseSchemaAsync(context);

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

        // Seed language options if missing
        await SeedLanguageOptionsAsync(context);
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

    private static bool _isSchemaEnsured = false;

    public static async Task EnsureDatabaseSchemaAsync(ApplicationDbContext context)
    {
        if (_isSchemaEnsured) return;
        try
        {
            context.Database.SetCommandTimeout(120);
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'StoryType')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD StoryType NVARCHAR(100) NOT NULL DEFAULT 'Novel'; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Tagline')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD Tagline NVARCHAR(500) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Synopsis')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD Synopsis NVARCHAR(MAX) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'PointOfView')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD PointOfView NVARCHAR(100) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'TimePeriod')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD TimePeriod NVARCHAR(100) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Language')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD Language NVARCHAR(100) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'TargetAudience')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD TargetAudience NVARCHAR(100) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Themes')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD Themes NVARCHAR(500) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Tone')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD Tone NVARCHAR(200) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'HeroBannerImageUrl')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD HeroBannerImageUrl NVARCHAR(500) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'IsBookmarked')
                BEGIN ALTER TABLE DI_TRN_WebStories ADD IsBookmarked BIT NOT NULL DEFAULT 0; END

                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('DI_TRN_WebStoryParts') AND type in (N'U'))
                BEGIN
                    CREATE TABLE DI_TRN_WebStoryParts (
                        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                        StoryId UNIQUEIDENTIFIER NOT NULL,
                        Title NVARCHAR(200) NOT NULL,
                        [Order] INT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT FK_DI_TRN_WebStoryParts_DI_TRN_WebStories_StoryId FOREIGN KEY (StoryId) REFERENCES DI_TRN_WebStories(Id) ON DELETE CASCADE
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'PartId')
                BEGIN
                    ALTER TABLE DI_TRN_WebChapters ADD PartId UNIQUEIDENTIFIER NULL;
                    ALTER TABLE DI_TRN_WebChapters ADD CONSTRAINT FK_DI_TRN_WebChapters_DI_TRN_WebStoryParts_PartId FOREIGN KEY (PartId) REFERENCES DI_TRN_WebStoryParts(Id) ON DELETE SET NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Status')
                BEGIN
                    ALTER TABLE DI_TRN_WebChapters ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Planned';
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'CharacterCount')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD CharacterCount INT NOT NULL DEFAULT 0; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Version')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD Version INT NOT NULL DEFAULT 1; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Summary')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD Summary NVARCHAR(MAX) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'TargetWordCount')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD TargetWordCount INT NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Purpose')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD Purpose NVARCHAR(MAX) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Goal')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD Goal NVARCHAR(MAX) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'KeyEvents')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD KeyEvents NVARCHAR(MAX) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'EmotionalTone')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD EmotionalTone NVARCHAR(500) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'PointOfView')
                BEGIN ALTER TABLE DI_TRN_WebChapters ADD PointOfView NVARCHAR(200) NULL; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WeeklyWordCountGoal')
                BEGIN ALTER TABLE DI_TRN_UserGoals ADD WeeklyWordCountGoal INT NOT NULL DEFAULT 5000; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WordsWrittenThisWeek')
                BEGIN ALTER TABLE DI_TRN_UserGoals ADD WordsWrittenThisWeek INT NOT NULL DEFAULT 0; END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WordsWrittenThisMonth')
                BEGIN ALTER TABLE DI_TRN_UserGoals ADD WordsWrittenThisMonth INT NOT NULL DEFAULT 0; END
            ");
            _isSchemaEnsured = true;
        }
        catch
        {
            // Ignore if columns already exist
        }
    }

    public static async Task<Story> SeedDilHaiKiMaantaNahiAsync(ApplicationDbContext context, string userId)
    {
        await EnsureDatabaseSchemaAsync(context);

        var story = await context.Stories
            .Include(s => s.StoryParts)
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Title == "Dil Hai Ki Maanta Nahi" && s.UserId == userId);

        if (story == null)
        {
            story = new Story
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Dil Hai Ki Maanta Nahi",
                StoryType = "Novel",
                Genre = "Fantasy • Romance • Drama",
                Tagline = "Some hearts write their own destiny.",
                Synopsis = "When destiny brings two broken souls together, the lines between love, loyalty and revenge blur.\n\nIn a world where secrets run deeper than blood, Sameer's quest for justice collides with Riya's fight for survival. As truth and betrayals surface, they must choose between the past that haunts them and the future they dare to dream.",
                PointOfView = "Third Person Limited",
                TimePeriod = "Modern",
                Language = "English",
                TargetAudience = "Young Adult",
                Themes = "Love, Fate, Sacrifice, Revenge",
                Tone = "Emotional, Intense, Romantic",
                CoverImageUrl = "/images/dil_hai_ki_cover.png",
                HeroBannerImageUrl = "/images/dil_hai_ki_hero_bg.png",
                TargetWordCount = 128828,
                CurrentWordCount = 82450,
                Status = "InProgress",
                CreatedAt = new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc)
            };
            context.Stories.Add(story);
            await context.SaveChangesAsync();
        }
        else
        {
            story.StoryType = "Novel";
            story.Genre = "Fantasy • Romance • Drama";
            story.Tagline = "Some hearts write their own destiny.";
            story.Synopsis = "When destiny brings two broken souls together, the lines between love, loyalty and revenge blur.\n\nIn a world where secrets run deeper than blood, Sameer's quest for justice collides with Riya's fight for survival. As truth and betrayals surface, they must choose between the past that haunts them and the future they dare to dream.";
            story.PointOfView = "Third Person Limited";
            story.TimePeriod = "Modern";
            story.Language = "English";
            story.TargetAudience = "Young Adult";
            story.Themes = "Love, Fate, Sacrifice, Revenge";
            story.Tone = "Emotional, Intense, Romantic";
            story.CoverImageUrl = "/images/dil_hai_ki_cover.png";
            story.HeroBannerImageUrl = "/images/dil_hai_ki_hero_bg.png";
            story.TargetWordCount = 128828;
            story.CurrentWordCount = 82450;
            story.Status = "InProgress";
            story.UpdatedAt = new DateTime(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc);
            await context.SaveChangesAsync();
        }

        // Link Genres
        var fantasyGenre = await context.Genres.FirstOrDefaultAsync(g => g.Slug == "fantasy");
        var romanceGenre = await context.Genres.FirstOrDefaultAsync(g => g.Slug == "romance");
        var dramaGenre = await context.Genres.FirstOrDefaultAsync(g => g.Slug == "drama");

        if (fantasyGenre != null && !await context.StoryGenres.AnyAsync(sg => sg.StoryId == story.Id && sg.GenreId == fantasyGenre.Id))
            context.StoryGenres.Add(new StoryGenre { StoryId = story.Id, GenreId = fantasyGenre.Id, IsPrimary = true, SortOrder = 0 });
        if (romanceGenre != null && !await context.StoryGenres.AnyAsync(sg => sg.StoryId == story.Id && sg.GenreId == romanceGenre.Id))
            context.StoryGenres.Add(new StoryGenre { StoryId = story.Id, GenreId = romanceGenre.Id, IsPrimary = false, SortOrder = 1 });
        if (dramaGenre != null && !await context.StoryGenres.AnyAsync(sg => sg.StoryId == story.Id && sg.GenreId == dramaGenre.Id))
            context.StoryGenres.Add(new StoryGenre { StoryId = story.Id, GenreId = dramaGenre.Id, IsPrimary = false, SortOrder = 2 });

        await context.SaveChangesAsync();

        // Seed Parts & Chapters if chapters count < 32
        var currentChaptersCount = await context.Chapters.CountAsync(c => c.StoryId == story.Id);
        if (currentChaptersCount < 32)
        {
            // Remove existing chapters/parts to re-seed cleanly
            var existingChs = await context.Chapters.Where(c => c.StoryId == story.Id).ToListAsync();
            context.Chapters.RemoveRange(existingChs);

            var existingParts = await context.StoryParts.Where(p => p.StoryId == story.Id).ToListAsync();
            context.StoryParts.RemoveRange(existingParts);

            await context.SaveChangesAsync();

            var p1 = new StoryPart { Id = Guid.NewGuid(), StoryId = story.Id, Title = "PART I - The Beginning", Order = 1 };
            var p2 = new StoryPart { Id = Guid.NewGuid(), StoryId = story.Id, Title = "PART II - The Revelation", Order = 2 };
            var p3 = new StoryPart { Id = Guid.NewGuid(), StoryId = story.Id, Title = "PART III - The Turning Point", Order = 3 };
            var p4 = new StoryPart { Id = Guid.NewGuid(), StoryId = story.Id, Title = "PART IV - The Truth", Order = 4 };
            var p5 = new StoryPart { Id = Guid.NewGuid(), StoryId = story.Id, Title = "PART V - The Destiny", Order = 5 };

            context.StoryParts.AddRange(p1, p2, p3, p4, p5);
            await context.SaveChangesAsync();

            // Seed Chapters
            var chapters = new List<Chapter>
            {
                // Part I (3 completed, 1 in progress, 1 planned)
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p1.Id, Order = 1, Title = "The First Night", WordCount = 2450, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p1.Id, Order = 2, Title = "A Stranger Arrives", WordCount = 3120, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p1.Id, Order = 3, Title = "The Letter", WordCount = 1840, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p1.Id, Order = 4, Title = "Beneath the Surface", WordCount = 1250, Status = "InProgress" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p1.Id, Order = 5, Title = "Whispers in the Dark", WordCount = 0, Status = "Planned" },

                // Part II (3 completed, 1 in progress)
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p2.Id, Order = 6, Title = "Unspoken Vows", WordCount = 2900, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p2.Id, Order = 7, Title = "Echoes of Betrayal", WordCount = 3400, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p2.Id, Order = 8, Title = "The Shadows Deepen", WordCount = 2800, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p2.Id, Order = 9, Title = "Fragments of Truth", WordCount = 1150, Status = "InProgress" },

                // Part III (4 completed, 1 in progress)
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p3.Id, Order = 10, Title = "Crossroads of Fate", WordCount = 3600, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p3.Id, Order = 11, Title = "Shattered Trust", WordCount = 3200, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p3.Id, Order = 12, Title = "The Hidden Cipher", WordCount = 2950, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p3.Id, Order = 13, Title = "Midnight Confessions", WordCount = 3100, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p3.Id, Order = 14, Title = "The Turning Tide", WordCount = 1100, Status = "InProgress" },

                // Part IV (6 completed)
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 15, Title = "Veil of Deception", WordCount = 3500, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 16, Title = "Trial by Fire", WordCount = 3800, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 17, Title = "Bound by Blood", WordCount = 3300, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 18, Title = "The Storm Gathers", WordCount = 3150, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 19, Title = "Reckoning at Dawn", WordCount = 3650, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p4.Id, Order = 20, Title = "Masks Fall Away", WordCount = 3700, Status = "Completed" },

                // Part V (2 completed, 1 in progress, 9 planned)
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 21, Title = "Fires of Vengeance", WordCount = 3900, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 22, Title = "Sacrifice", WordCount = 3850, Status = "Completed" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 23, Title = "The Final Stand", WordCount = 1240, Status = "InProgress" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 24, Title = "Shadows in the Mist", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 25, Title = "The Silent Promise", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 26, Title = "Echoes of Eternity", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 27, Title = "Beyond the Veil", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 28, Title = "The Lost Kingdom", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 29, Title = "Dawn of Hope", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 30, Title = "Unbroken Chains", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 31, Title = "The Last Chapter", WordCount = 0, Status = "Planned" },
                new Chapter { Id = Guid.NewGuid(), StoryId = story.Id, PartId = p5.Id, Order = 32, Title = "Epilogue - Destiny Written", WordCount = 0, Status = "Planned" }
            };

            context.Chapters.AddRange(chapters);
            await context.SaveChangesAsync();
        }

        // Seed 23 Characters if count < 23
        var existingCharCount = await context.Characters.CountAsync(c => c.StoryId == story.Id);
        if (existingCharCount < 23)
        {
            var charNames = new[] {
                "Sameer Malhotra", "Riya", "Raj Malhotra", "Meera Malhotra", "Inspector Sharma",
                "Kael Draven", "Alaric Vayne", "Seraphina Lorne", "Elara Moonwhisper", "Lord Marshall Eldric",
                "Lira Dashwood", "Vikram Rathore", "Aanya Verma", "Devendra Roy", "Maya Sengupta",
                "Kabir Anand", "Zoya Khan", "Tariq Mahmood", "Naina Kapoor", "Rohan Mehta",
                "Priya Joshi", "Arjun Singhania", "Divya Thakur"
            };

            for (int i = existingCharCount; i < 23; i++)
            {
                context.Characters.Add(new Character
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Name = charNames[i],
                    Role = i < 2 ? "Protagonist" : (i < 5 ? "Supporting" : "Secondary"),
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed 18 Locations if count < 18
        var existingLocCount = await context.Locations.CountAsync(l => l.StoryId == story.Id);
        if (existingLocCount < 18)
        {
            var locNames = new[] {
                "Silverbrook City", "Dockside Area", "High Court of Silverbrook", "Old Market",
                "Crime Branch Headquarters", "Silverbrook University Library", "Malhotra Villa",
                "Grand Palace Ruins", "Shadow Alley", "Eldoria Forest",
                "Ravenhold Castle", "Whispering Falls", "Starlight Observatory", "Blackwood Manor",
                "Emerald Harbor", "Sunspire Tower", "Ironhold Prison", "Sanctuary Cove"
            };

            for (int i = existingLocCount; i < 18; i++)
            {
                context.Locations.Add(new Location
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Name = locNames[i],
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed 24 Timeline Events if count < 24
        var existingEventCount = await context.TimelineEvents.CountAsync(e => e.StoryId == story.Id);
        if (existingEventCount < 24)
        {
            for (int i = existingEventCount; i < 24; i++)
            {
                context.TimelineEvents.Add(new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Title = $"Timeline Event {i + 1}",
                    Category = i % 2 == 0 ? "Investigation" : "Incident",
                    StoryDate = $"Day {i + 1}",
                    DisplayOrder = i + 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed 4 Story Arcs if count < 4
        var existingArcCount = await context.StoryArcs.CountAsync(a => a.StoryId == story.Id);
        if (existingArcCount < 4)
        {
            var arcTitles = new[] { "Main Story Arc", "Romance Arc", "Mystery Arc", "Crime Investigation" };
            for (int i = existingArcCount; i < 4; i++)
            {
                context.StoryArcs.Add(new StoryArc
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Title = arcTitles[i],
                    ArcType = arcTitles[i],
                    Status = "Active",
                    DisplayOrder = i + 1
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed 12 Research Notes if count < 12
        var existingNoteCount = await context.ResearchNotes.CountAsync(r => r.StoryId == story.Id);
        if (existingNoteCount < 12)
        {
            for (int i = existingNoteCount; i < 12; i++)
            {
                context.ResearchNotes.Add(new ResearchNote
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Title = $"Research Item {i + 1}",
                    Category = i < 4 ? "Historical" : "Setting",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed 23 Assets if count < 23
        var existingAssetCount = await context.Assets.CountAsync(a => a.StoryId == story.Id);
        if (existingAssetCount < 23)
        {
            for (int i = existingAssetCount; i < 23; i++)
            {
                context.Assets.Add(new Asset
                {
                    Id = Guid.NewGuid(),
                    StoryId = story.Id,
                    Title = $"Asset File {i + 1}",
                    Type = i % 2 == 0 ? "Image" : "Document",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Update UserGoal for user
        var userGoal = await context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
        if (userGoal == null)
        {
            userGoal = new UserGoal
            {
                UserId = userId,
                DailyWordCountGoal = 1000,
                WeeklyWordCountGoal = 5000,
                MonthlyWordCountGoal = 20000,
                WordsWrittenToday = 1250,
                WordsWrittenThisWeek = 8200,
                WordsWrittenThisMonth = 20450,
                CurrentStreakDays = 12,
                LastUpdated = DateTime.UtcNow
            };
            context.UserGoals.Add(userGoal);
        }
        else
        {
            userGoal.DailyWordCountGoal = 1000;
            userGoal.WeeklyWordCountGoal = 5000;
            userGoal.MonthlyWordCountGoal = 20000;
            userGoal.WordsWrittenToday = 1250;
            userGoal.WordsWrittenThisWeek = 8200;
            userGoal.WordsWrittenThisMonth = 20450;
            userGoal.CurrentStreakDays = 12;
            userGoal.LastUpdated = DateTime.UtcNow;
        }
        await context.SaveChangesAsync();

        return story;
    }

    public static async Task SeedLanguageOptionsAsync(ApplicationDbContext context)
    {
        var languagesToAdd = new[]
        {
            new DropdownOption { Id = Guid.NewGuid(), Category = "Language", Value = "HindiInEnglish", Text = "Hindi - Written in English", Description = "Hindi written using Roman / English script.", DisplayOrder = 3, IsActive = true },
            new DropdownOption { Id = Guid.NewGuid(), Category = "Language", Value = "PureHindi", Text = "Pure Hindi", Description = "Pure Hindi language written in Devanagari script.", DisplayOrder = 4, IsActive = true },
            new DropdownOption { Id = Guid.NewGuid(), Category = "Language", Value = "HindiAndEnglish", Text = "Hindi + English", Description = "Combination of Hindi and English language.", DisplayOrder = 5, IsActive = true }
        };

        foreach (var lang in languagesToAdd)
        {
            if (!await context.DropdownOptions.AnyAsync(d => d.Category == "Language" && d.Value == lang.Value))
            {
                context.DropdownOptions.Add(lang);
            }
        }

        await context.SaveChangesAsync();
    }
}
