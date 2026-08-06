using System;
using System.Collections.Generic;
using StoryVerse.Core.Entities.Identity;

namespace StoryVerse.Core.Entities
{
    public class Story
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int TargetWordCount { get; set; }
        public int CurrentWordCount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, InProgress, Published
        
        // Future Architecture Readiness (Series, Universes, Branching, Sequels)
        public Guid? SeriesId { get; set; }
        public Guid? UniverseId { get; set; }
        public Guid? ParentStoryId { get; set; }
        public bool IsBranch { get; set; } = false;
        public string? BranchName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int ProgressPercentage => TargetWordCount == 0 ? 0 : (int)Math.Clamp(((double)CurrentWordCount / TargetWordCount) * 100, 0, 100);

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<Character> Characters { get; set; } = new List<Character>();
        public ICollection<Location> Locations { get; set; } = new List<Location>();
        public ICollection<TimelineEvent> TimelineEvents { get; set; } = new List<TimelineEvent>();
        public ICollection<StoryArc> StoryArcs { get; set; } = new List<StoryArc>();
        public ICollection<StoryTimeline> StoryTimelines { get; set; } = new List<StoryTimeline>();
        public ICollection<ResearchNote> ResearchNotes { get; set; } = new List<ResearchNote>();
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }

    public class Chapter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;
        
        public string Title { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public int Order { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Character
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story? Story { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? ArcType { get; set; }
        public string? Nicknames { get; set; }
        public string? Age { get; set; }
        public string? Gender { get; set; }
        public string? Pronouns { get; set; }
        public string? Occupation { get; set; }
        public string? Status { get; set; }
        public string? Alignment { get; set; }
        public string? OneLineDescription { get; set; }
        public string? BackgroundSummary { get; set; }
        public string? Tags { get; set; }
        public string? AvatarUrl { get; set; }

        // Step 2: Appearance
        public string? Height { get; set; }
        public string? Build { get; set; }
        public string? Complexion { get; set; }
        public string? EyeColor { get; set; }
        public string? HairColor { get; set; }
        public string? HairStyle { get; set; }
        public string? DistinguishingFeatures { get; set; }
        public string? ClothingStyle { get; set; }
        public string? PreferredColors { get; set; }
        public string? Accessories { get; set; }
        public string? VoiceTone { get; set; }
        public string? Accent { get; set; }
        public string? SpeechPattern { get; set; }
        public string? AppearanceNotes { get; set; }

        // Step 3: Personality
        public string? PersonalityTraits { get; set; }
        public string? PersonalityOverview { get; set; }
        public string? ValuesBeliefs { get; set; }
        public string? Strengths { get; set; }
        public string? Motivations { get; set; }
        public string? Temperament { get; set; }
        public string? Flaws { get; set; }
        public string? Fears { get; set; }
        public string? Desires { get; set; }

        // Step 4: Background
        public string? PlaceOfBirth { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? FamilyBackground { get; set; }
        public string? Upbringing { get; set; }
        public string? Education { get; set; }
        public string? KeyEvents { get; set; }
        public string? Backstory { get; set; }
        public string? SocioeconomicStatus { get; set; }
        public string? CurrentResidence { get; set; }
        public string? Languages { get; set; }
        public string? BackgroundDocumentUrl { get; set; }

        // Step 5: Relationships
        public string? Allies { get; set; }
        public string? Enemies { get; set; }
        public string? Family { get; set; }
        public string? LoveInterests { get; set; }
        public string? RelationshipsJson { get; set; }
        public string? RelationshipChartUrl { get; set; }

        // Step 6: Review & Additional Details
        public string? AuthorNotes { get; set; }
        public string? FamilyCrest { get; set; }
        public string? ThemeColor { get; set; }
        public string? CustomDocumentUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Location
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;
        
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ActivityLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public string ActionType { get; set; } = string.Empty; // Chapter, Character, Location, Story
        public string Description { get; set; } = string.Empty;
        public string RelatedEntityName { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class UserGoal
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public int DailyWordCountGoal { get; set; } = 1000;
        public int WordsWrittenToday { get; set; } = 0;
        public int CurrentStreakDays { get; set; } = 0;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
