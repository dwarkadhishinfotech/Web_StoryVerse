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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int ProgressPercentage => TargetWordCount == 0 ? 0 : (int)Math.Clamp(((double)CurrentWordCount / TargetWordCount) * 100, 0, 100);

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<Character> Characters { get; set; } = new List<Character>();
        public ICollection<Location> Locations { get; set; } = new List<Location>();
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
        public Story Story { get; set; } = null!;
        
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ArcType { get; set; } = string.Empty;
        public string Nicknames { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Pronouns { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Alignment { get; set; } = string.Empty;
        public string OneLineDescription { get; set; } = string.Empty;
        public string BackgroundSummary { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        // Step 2: Appearance
        public string Height { get; set; } = string.Empty;
        public string Build { get; set; } = string.Empty;
        public string Complexion { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string HairStyle { get; set; } = string.Empty;
        public string DistinguishingFeatures { get; set; } = string.Empty;
        public string ClothingStyle { get; set; } = string.Empty;
        public string PreferredColors { get; set; } = string.Empty;
        public string Accessories { get; set; } = string.Empty;
        public string VoiceTone { get; set; } = string.Empty;
        public string Accent { get; set; } = string.Empty;
        public string SpeechPattern { get; set; } = string.Empty;
        public string AppearanceNotes { get; set; } = string.Empty;

        // Step 3: Personality
        public string PersonalityTraits { get; set; } = string.Empty;
        public string PersonalityOverview { get; set; } = string.Empty;
        public string ValuesBeliefs { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string Motivations { get; set; } = string.Empty;
        public string Temperament { get; set; } = string.Empty;
        public string Flaws { get; set; } = string.Empty;
        public string Fears { get; set; } = string.Empty;
        public string Desires { get; set; } = string.Empty;

        // Step 4: Background
        public string PlaceOfBirth { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string FamilyBackground { get; set; } = string.Empty;
        public string Upbringing { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string KeyEvents { get; set; } = string.Empty;
        public string Backstory { get; set; } = string.Empty;
        public string SocioeconomicStatus { get; set; } = string.Empty;
        public string CurrentResidence { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string BackgroundDocumentUrl { get; set; } = string.Empty;

        // Step 5: Relationships
        public string Allies { get; set; } = string.Empty;
        public string Enemies { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string LoveInterests { get; set; } = string.Empty;
        public string RelationshipsJson { get; set; } = string.Empty;
        public string RelationshipChartUrl { get; set; } = string.Empty;

        // Step 6: Review & Additional Details
        public string AuthorNotes { get; set; } = string.Empty;
        public string FamilyCrest { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = string.Empty;
        public string CustomDocumentUrl { get; set; } = string.Empty;
        
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
