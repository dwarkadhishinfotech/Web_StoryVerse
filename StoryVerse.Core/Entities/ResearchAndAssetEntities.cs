using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    public class ResearchNote
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string Category { get; set; } = "General"; // Historical, Science, Character Background, Setting, Magic System, Plot, Legal, Medical, Custom
        public string? Tags { get; set; }
        public string? SourceUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ResearchCharacter> CharacterLinks { get; set; } = new List<ResearchCharacter>();
        public ICollection<ResearchWorldEntity> WorldEntityLinks { get; set; } = new List<ResearchWorldEntity>();
        public ICollection<ResearchTimelineEvent> TimelineEventLinks { get; set; } = new List<ResearchTimelineEvent>();
        public ICollection<ResearchChapter> ChapterLinks { get; set; } = new List<ResearchChapter>();
        public ICollection<ResearchAsset> AssetLinks { get; set; } = new List<ResearchAsset>();
    }

    public class ResearchCharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;
    }

    public class ResearchWorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
        public Guid WorldEntityId { get; set; }
        public WorldEntity WorldEntity { get; set; } = null!;
    }

    public class ResearchTimelineEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;
    }

    public class ResearchChapter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;
    }

    public class ResearchAsset
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
    }

    public class Asset
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "Image"; // Image, Document, Video, Audio, Map, Sketch, Reference Material
        public string FileUrl { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public long FileSize { get; set; } = 0;
        public string? MimeType { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AssetCharacter> CharacterLinks { get; set; } = new List<AssetCharacter>();
        public ICollection<AssetWorldEntity> WorldEntityLinks { get; set; } = new List<AssetWorldEntity>();
        public ICollection<AssetTimelineEvent> TimelineEventLinks { get; set; } = new List<AssetTimelineEvent>();
        public ICollection<AssetChapter> ChapterLinks { get; set; } = new List<AssetChapter>();
        public ICollection<AssetResearchNote> ResearchLinks { get; set; } = new List<AssetResearchNote>();
    }

    public class AssetCharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;
    }

    public class AssetWorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
        public Guid WorldEntityId { get; set; }
        public WorldEntity WorldEntity { get; set; } = null!;
    }

    public class AssetTimelineEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
        public Guid TimelineEventId { get; set; }
        public TimelineEvent TimelineEvent { get; set; } = null!;
    }

    public class AssetChapter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;
    }

    public class AssetResearchNote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
        public Guid ResearchNoteId { get; set; }
        public ResearchNote ResearchNote { get; set; } = null!;
    }
}
