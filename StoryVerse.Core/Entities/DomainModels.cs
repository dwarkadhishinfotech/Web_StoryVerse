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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
