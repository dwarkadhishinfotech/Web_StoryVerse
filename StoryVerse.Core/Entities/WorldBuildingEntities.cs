using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    public class WorldEntityType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // Location, Organization, Religion, Species, Magic System, etc.
        public string Category { get; set; } = "Locations"; // Locations, Organizations, People Groups, Cultures, Religions, Species, Historical Events, Other
        public string Icon { get; set; } = "folder"; // Lucide icon name
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsSystemDefault { get; set; } = true;
        public string? UserId { get; set; } // Null for default system types, or UserId if custom

        public ICollection<WorldEntityField> Fields { get; set; } = new List<WorldEntityField>();
        public ICollection<WorldEntity> Entities { get; set; } = new List<WorldEntity>();
    }

    public class WorldEntityField
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityTypeId { get; set; }
        public WorldEntityType EntityType { get; set; } = null!;

        public string FieldName { get; set; } = string.Empty; // e.g. Population, Climate, Coordinates, Ruler, Government, Currency
        public string FieldType { get; set; } = "Text"; // Text, Number, Textarea, Select, Boolean, Date, Url
        public bool Required { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? OptionsJson { get; set; } // Options for Select field type

        public ICollection<WorldEntityValue> FieldValues { get; set; } = new List<WorldEntityValue>();
    }

    public class WorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public Guid EntityTypeId { get; set; }
        public WorldEntityType EntityType { get; set; } = null!;

        public Guid? ParentEntityId { get; set; }
        public WorldEntity? ParentEntity { get; set; }
        public ICollection<WorldEntity> SubEntities { get; set; } = new List<WorldEntity>();

        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; } // Specific icon or null to fallback to EntityType.Icon
        public string? CoverImage { get; set; }
        public string Status { get; set; } = "Active"; // Active, Draft, Archived
        public string Importance { get; set; } = "Major"; // Minor, Major, Critical
        public string? Color { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsFavorite { get; set; } = false;
        public string? Tags { get; set; } // Comma-separated or space-separated tags

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool ActiveStatus { get; set; } = true;

        public ICollection<WorldEntityValue> FieldValues { get; set; } = new List<WorldEntityValue>();
        public ICollection<WorldEntityRelationship> SourceRelationships { get; set; } = new List<WorldEntityRelationship>();
        public ICollection<WorldEntityRelationship> TargetRelationships { get; set; } = new List<WorldEntityRelationship>();
        public ICollection<WorldEntityCharacter> CharacterLinks { get; set; } = new List<WorldEntityCharacter>();
        public ICollection<WorldEntityTimeline> TimelineLinks { get; set; } = new List<WorldEntityTimeline>();
    }

    public class WorldEntityValue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityId { get; set; }
        public WorldEntity Entity { get; set; } = null!;

        public Guid FieldId { get; set; }
        public WorldEntityField Field { get; set; } = null!;

        public string? Value { get; set; }
    }

    public class WorldEntityRelationship
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SourceEntityId { get; set; }
        public WorldEntity SourceEntity { get; set; } = null!;

        public Guid TargetEntityId { get; set; }
        public WorldEntity TargetEntity { get; set; } = null!;

        public string RelationshipType { get; set; } = "Located In"; // Located In, Enemy Of, Allied With, Member Of, Ruled By, Founded By, Lives In, Works At, Controls, Protects, Destroyed By, Created By, Custom
        public string? Description { get; set; }
    }

    public class WorldEntityCharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EntityId { get; set; }
        public WorldEntity Entity { get; set; } = null!;

        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public string RelationshipRole { get; set; } = "Member"; // Birthplace, Residence, School, Organization, Religion, Species, Workplace, Leader, Ruler, Founder
    }

    public class WorldEntityTimeline
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EntityId { get; set; }
        public WorldEntity Entity { get; set; } = null!;

        public string EventTitle { get; set; } = string.Empty;
        public string? EventDate { get; set; }
        public string? Description { get; set; }
    }

    public class WorldTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // e.g. Fantasy Kingdom, Modern City, Crime World, Sci-Fi Universe, Historical Realm
        public string Genre { get; set; } = "Fantasy"; // Fantasy, Crime Thriller, Mystery, Romance, Historical, Sci-Fi, Horror, Contemporary, Adventure, Custom
        public string? Description { get; set; }
        public string Icon { get; set; } = "book-open";
        public string SubTypesSummary { get; set; } = string.Empty; // e.g. "Kingdom, City, Castle, Village, Guild..."
    }

    public class WorldMap
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string MapType { get; set; } = "World"; // World, City, Building, Kingdom, Galaxy
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorldMapMarker> Markers { get; set; } = new List<WorldMapMarker>();
    }

    public class WorldMapMarker
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MapId { get; set; }
        public WorldMap Map { get; set; } = null!;

        public Guid? EntityId { get; set; }
        public WorldEntity? Entity { get; set; }

        public double XCoord { get; set; }
        public double YCoord { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
