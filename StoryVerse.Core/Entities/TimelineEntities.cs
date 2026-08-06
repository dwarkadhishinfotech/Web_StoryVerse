using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    public class TimelineEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Core Aggregate Root Ownership: Story
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }

        // Event Categories: Birth, Death, Marriage, Crime, Meeting, Career, Investigation, Incident, Battle, Journey, Mission, Discovery, Political Event, Flashback, Future Event, Dream, Vision, Prophecy, Celebration, Custom
        public string Category { get; set; } = "General";
        public string EventType { get; set; } = "Standard"; // Standard, Milestone, Climax, Backstory, Subplot
        
        public DateTime? RealDate { get; set; }
        public string? StoryDate { get; set; } // e.g. "12 Jan 2020", "Year 302", "Day 14"
        public string? LocationName { get; set; } // Quick display location string

        public int DisplayOrder { get; set; } = 0;
        public string Importance { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Status { get; set; } = "Confirmed"; // Draft, Confirmed, Archived
        public string? Color { get; set; } // Hex or Tailwind badge color
        public string? Icon { get; set; } // Lucide icon identifier
        public string? ImpactNotes { get; set; }
        public bool IsBookmarked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Relationships
        public ICollection<TimelineCharacter> CharacterLinks { get; set; } = new List<TimelineCharacter>();
        public ICollection<TimelineWorldEntity> WorldEntityLinks { get; set; } = new List<TimelineWorldEntity>();
        public ICollection<TimelineRelationship> SourceRelationships { get; set; } = new List<TimelineRelationship>();
        public ICollection<TimelineRelationship> TargetRelationships { get; set; } = new List<TimelineRelationship>();
        public ICollection<TimelineEventChapter> ChapterLinks { get; set; } = new List<TimelineEventChapter>();
        public ICollection<StoryArcEvent> ArcLinks { get; set; } = new List<StoryArcEvent>();
        public ICollection<ResearchTimelineEvent> ResearchLinks { get; set; } = new List<ResearchTimelineEvent>();
        public ICollection<AssetTimelineEvent> AssetLinks { get; set; } = new List<AssetTimelineEvent>();
    }

    public class TimelineCharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;

        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public string Role { get; set; } = "Participant"; // Victim, Witness, Detective, Participant, Suspect, Leader, Narrator, Primary, Secondary
        public string? Notes { get; set; }
    }

    public class TimelineWorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;

        public Guid WorldEntityId { get; set; }
        public WorldEntity WorldEntity { get; set; } = null!;

        public string Role { get; set; } = "Location"; // Location, Setting, Affected Entity, Battlefield, Headquarters
        public string? Notes { get; set; }
    }

    public class TimelineRelationship
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SourceEventId { get; set; }
        public TimelineEvent SourceEvent { get; set; } = null!;

        public Guid TargetEventId { get; set; }
        public TimelineEvent TargetEvent { get; set; } = null!;

        public string RelationshipType { get; set; } = "Precedes"; // Causes, Precedes, Enables, Triggers, Follows, Parallel, Conflicts
        public string? Notes { get; set; }
    }

    public class TimelineEventChapter
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;

        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        public string? Notes { get; set; }
    }

    public class StoryArc
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string ArcType { get; set; } = "Main Story"; // Main Story, Character Arc, Romance Arc, Mystery Arc, Political Arc, War Arc, Crime Investigation, Past Trauma Arc, Redemption Arc, Custom
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Planned
        public int TargetCompletionPercent { get; set; } = 0;
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StoryArcEvent> ArcEvents { get; set; } = new List<StoryArcEvent>();
    }

    public class StoryArcEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryArcId { get; set; }
        public StoryArc StoryArc { get; set; } = null!;

        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;

        public int OrderInArc { get; set; } = 0;
        public string? ImpactLevel { get; set; } // Minor, Major, Climax
        public string? Notes { get; set; }
    }

    public class StoryTimeline
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "Teal"; // Teal, Blue, Purple, Magenta, Red, Orange, Yellow, Slate
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Tags { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = "Active"; // Active, Draft, Archived

        // Step 2: Timeline Type
        public string TimelineType { get; set; } = "Chronological Timeline";

        // Step 3: Date & Time Settings
        public string DateFormat { get; set; } = "DD MMM YYYY (31 Dec 2025)";
        public string TimeFormat { get; set; } = "12 Hour (AM/PM)";
        public string DefaultTime { get; set; } = "12:00 PM";
        public string CalendarStartDay { get; set; } = "Monday";
        public string TimeZone { get; set; } = "(GMT+05:30) Asia/Kolkata";

        // Step 3: Display Settings
        public string DefaultTimelineView { get; set; } = "Chronological Timeline";
        public string EventGrouping { get; set; } = "Group by Date";
        public bool ShowTimeOnTimeline { get; set; } = true;
        public bool ShowEventIcons { get; set; } = true;
        public bool ShowEventDescriptions { get; set; } = true;
        public bool CompactMode { get; set; } = false;

        // Step 3: Behavior Settings
        public bool AllowOverlappingEvents { get; set; } = true;
        public bool EnableTimelineDependencies { get; set; } = true;
        public bool AutoSortNewEvents { get; set; } = true;
        public bool EnableReminders { get; set; } = true;
        public bool LockTimelineDates { get; set; } = false;
        public bool ShowFutureEvents { get; set; } = true;
        public bool ShowCompletedEvents { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TimelineStoryArc> LinkedStoryArcs { get; set; } = new List<TimelineStoryArc>();
    }

    public class TimelineStoryArc
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryTimelineId { get; set; }
        public StoryTimeline StoryTimeline { get; set; } = null!;

        public Guid StoryArcId { get; set; }
        public StoryArc StoryArc { get; set; } = null!;
    }
}
