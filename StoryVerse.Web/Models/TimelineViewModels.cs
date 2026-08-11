using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using StoryVerse.Core.Entities;

namespace StoryVerse.Web.Models
{
    public class TimelineStudioViewModel
    {
        public Guid SelectedStoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string StoryGenre { get; set; } = string.Empty;
        public List<StoryOptionDto> Stories { get; set; } = new List<StoryOptionDto>();

        public Guid? SelectedTimelineId { get; set; }
        public List<TimelineDto> Timelines { get; set; } = new List<TimelineDto>();

        // Statistics
        public int TotalEventsCount { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int HistoricalEventsCount { get; set; }
        public int StoryArcsCount { get; set; }
        public int CharactersInvolvedCount { get; set; }
        public int LocationsInvolvedCount { get; set; }

        // Active Navigation Tab & View Toggles
        public string ActiveTab { get; set; } = "TimelineView"; // TimelineView, CalendarView, ArcView, CharacterView, LocationView, InvestigationBoard
        public string ViewMode { get; set; } = "Vertical"; // Vertical, Horizontal, List, Graph
        public string SelectedCategory { get; set; } = "All Events";
        public string SearchQuery { get; set; } = string.Empty;

        // Data Lists
        public List<TimelineEventDto> Events { get; set; } = new List<TimelineEventDto>();
        public List<StoryArcDto> StoryArcs { get; set; } = new List<StoryArcDto>();
        public List<TimelineEventDto> UpcomingEvents { get; set; } = new List<TimelineEventDto>();

        // Entity Select Options for Wizard / Forms
        public List<CharacterOptionDto> Characters { get; set; } = new List<CharacterOptionDto>();
        public List<WorldEntityOptionDto> WorldEntities { get; set; } = new List<WorldEntityOptionDto>();
        public List<ChapterOptionDto> Chapters { get; set; } = new List<ChapterOptionDto>();
        public List<ResearchOptionDto> ResearchNotes { get; set; } = new List<ResearchOptionDto>();
        public List<AssetOptionDto> Assets { get; set; } = new List<AssetOptionDto>();
    }

    public class TimelineDto
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "Teal";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Tags { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = "Active";
        public string TimelineType { get; set; } = "Chronological Timeline";
        public string DateFormat { get; set; } = "DD MMM YYYY (31 Dec 2025)";
        public string TimeFormat { get; set; } = "12 Hour (AM/PM)";
        public string DefaultTime { get; set; } = "12:00 PM";
        public string CalendarStartDay { get; set; } = "Monday";
        public string TimeZone { get; set; } = "(GMT+05:30) Asia/Kolkata";
        public string DefaultTimelineView { get; set; } = "Chronological Timeline";
        public string EventGrouping { get; set; } = "Group by Date";
        public bool ShowTimeOnTimeline { get; set; } = true;
        public bool ShowEventIcons { get; set; } = true;
        public bool ShowEventDescriptions { get; set; } = true;
        public bool CompactMode { get; set; } = false;
        public bool AllowOverlappingEvents { get; set; } = true;
        public bool EnableTimelineDependencies { get; set; } = true;
        public bool AutoSortNewEvents { get; set; } = true;
        public bool EnableReminders { get; set; } = true;
        public bool LockTimelineDates { get; set; } = false;
        public bool ShowFutureEvents { get; set; } = true;
        public bool ShowCompletedEvents { get; set; } = true;
        public int EventCount { get; set; }
        public int StoryArcCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<StoryArcOptionDto> LinkedStoryArcs { get; set; } = new List<StoryArcOptionDto>();
    }

    public class StoryArcOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArcType { get; set; } = "Main Story";
        public string? Description { get; set; }
        public string Color { get; set; } = "#13A8A6";
        public int ProgressPercent { get; set; }
        public bool IsSelected { get; set; }
    }

    public class TimelineFormViewModel
    {
        public bool IsEdit { get; set; }
        public Guid? TimelineId { get; set; }
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string? StoryGenre { get; set; }
        public string? StoryCoverImageUrl { get; set; }

        // Step 1: Basic Information
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "Teal"; // Teal, Blue, Purple, Magenta, Red, Orange, Yellow, Slate
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Tags { get; set; }
        public string? CoverImageUrl { get; set; }
        public IFormFile? BannerFile { get; set; }

        // Step 2: Choose Timeline Type
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

        // Step 4: Story Arcs
        public List<Guid> SelectedStoryArcIds { get; set; } = new List<Guid>();

        // Select Lists for Wizard
        public List<StoryOptionDto> Stories { get; set; } = new List<StoryOptionDto>();
        public List<StoryArcOptionDto> AvailableStoryArcs { get; set; } = new List<StoryArcOptionDto>();
    }

    public class StoryOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    public class CharacterOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class WorldEntityOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }

    public class ChapterOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class ResearchOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class AssetOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class TimelineEventDto
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = "General"; // Birth, Meeting, Career, Investigation, Incident, Battle, etc.
        public string EventType { get; set; } = "Standard";
        public DateTime? RealDate { get; set; }
        public string? StoryDate { get; set; } // "12 Jan 2020", "14 Feb 2020", etc.
        public string DateYear { get; set; } = string.Empty; // e.g. "2020"
        public string DateMonthDay { get; set; } = string.Empty; // e.g. "JAN 12"
        public string? LocationName { get; set; }
        public string Importance { get; set; } = "Medium";
        public string Status { get; set; } = "Confirmed";
        public string Color { get; set; } = "#13A8A6";
        public string Icon { get; set; } = "calendar";
        public bool IsBookmarked { get; set; }

        public List<CharacterOptionDto> Characters { get; set; } = new List<CharacterOptionDto>();
        public List<WorldEntityOptionDto> WorldEntities { get; set; } = new List<WorldEntityOptionDto>();
        public List<ChapterOptionDto> Chapters { get; set; } = new List<ChapterOptionDto>();
    }

    public class StoryArcDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArcType { get; set; } = "Main Story";
        public string Color { get; set; } = "#13A8A6";
        public int ProgressPercent { get; set; }
        public int EventCount { get; set; }
    }

    public class TimelineEventInputModel
    {
        public Guid? Id { get; set; }
        public Guid StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
        public string EventType { get; set; } = "Standard";
        public DateTime? RealDate { get; set; }
        public string? StoryDate { get; set; }
        public string? LocationName { get; set; }
        public string Importance { get; set; } = "Medium";
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public List<Guid> SelectedCharacterIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedWorldEntityIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedChapterIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedResearchNoteIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedAssetIds { get; set; } = new List<Guid>();
    }

    public class StoryArcInputModel
    {
        public Guid? Id { get; set; }
        public Guid StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArcType { get; set; } = "Main Story";
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int TargetCompletionPercent { get; set; } = 50;
    }
}
