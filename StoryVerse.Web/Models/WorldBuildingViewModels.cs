using System;
using System.Collections.Generic;
using StoryVerse.Core.Entities;

namespace StoryVerse.Web.Models
{
    public class WorldBuildingIndexViewModel
    {
        public Story? Story { get; set; }
        public List<Story> UserStories { get; set; } = new List<Story>();
        public Guid? SelectedStoryId { get; set; }

        public int TotalEntities { get; set; }
        public int LocationsCount { get; set; }
        public int OrganizationsCount { get; set; }
        public int PeopleGroupsCount { get; set; }
        public int HistoricalEventsCount { get; set; }

        public string ActiveCategory { get; set; } = "All Entities";
        public string ActiveView { get; set; } = "grid"; // grid, list
        public string? SearchQuery { get; set; }
        public Guid? SelectedTypeId { get; set; }
        public string? SelectedStatus { get; set; }
        public string SortBy { get; set; } = "Recently Updated";

        public List<WorldEntityItemViewModel> Entities { get; set; } = new List<WorldEntityItemViewModel>();
        public List<WorldEntityType> SystemTypes { get; set; } = new List<WorldEntityType>();
        public List<WorldTemplate> SuggestedTemplates { get; set; } = new List<WorldTemplate>();
        
        public WorldEntityDetailViewModel? SelectedEntityDetail { get; set; }
    }

    public class WorldEntityItemViewModel
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Icon { get; set; } = "globe";
        public string CoverImage { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public string Status { get; set; } = "Active";
        public string Importance { get; set; } = "Major";
        public int ConnectedCharactersCount { get; set; }
        public int ConnectedTimelineEventsCount { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedAgo { get; set; } = string.Empty;
    }

    public class WorldEntityDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Icon { get; set; } = "globe";
        public string CoverImage { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Importance { get; set; } = "Major";
        public bool IsFavorite { get; set; }
        public string Quote { get; set; } = string.Empty;
        public List<FieldValueItemViewModel> FieldValues { get; set; } = new List<FieldValueItemViewModel>();
        public List<RelationshipItemViewModel> Relationships { get; set; } = new List<RelationshipItemViewModel>();
        public List<CharacterLinkItemViewModel> Characters { get; set; } = new List<CharacterLinkItemViewModel>();
        public List<TimelineLinkItemViewModel> TimelineEvents { get; set; } = new List<TimelineLinkItemViewModel>();
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime UpdatedDate { get; set; }
    }

    public class FieldValueItemViewModel
    {
        public Guid FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = "Text";
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = "info";
    }

    public class RelationshipItemViewModel
    {
        public Guid TargetEntityId { get; set; }
        public string TargetEntityName { get; set; } = string.Empty;
        public string TargetTypeName { get; set; } = string.Empty;
        public string TargetIcon { get; set; } = "link";
        public string RelationshipType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CharacterLinkItemViewModel
    {
        public Guid CharacterId { get; set; }
        public string CharacterName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string RelationshipRole { get; set; } = string.Empty;
    }

    public class TimelineLinkItemViewModel
    {
        public Guid Id { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateWorldEntityInputModel
    {
        public Guid StoryId { get; set; }
        public Guid EntityTypeId { get; set; }
        public Guid? ParentEntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public string Importance { get; set; } = "Major";
        public string? Icon { get; set; }
        public string? CoverImage { get; set; }
        public string? Tags { get; set; }

        public string? Population { get; set; }
        public string? Founded { get; set; }
        public string? Government { get; set; }
        public string? Ruler { get; set; }
        public string? Currency { get; set; }
        public string? Languages { get; set; }
        public string? Climate { get; set; }
        public string? TimeZone { get; set; }

        public List<FieldValueInput> Fields { get; set; } = new List<FieldValueInput>();
    }

    public class FieldValueInput
    {
        public Guid FieldId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class CreateWorldEntityViewModel
    {
        public Guid SelectedStoryId { get; set; }
        public Guid SelectedTypeId { get; set; }
        public Guid? ParentEntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public string Importance { get; set; } = "Major";
        public string? Icon { get; set; } = "map-pin";
        public string? CoverImage { get; set; }
        public string? Tags { get; set; }

        public List<Story> UserStories { get; set; } = new List<Story>();
        public List<WorldEntityType> EntityTypes { get; set; } = new List<WorldEntityType>();
        public List<WorldEntity> ParentEntities { get; set; } = new List<WorldEntity>();
        public List<Character> Characters { get; set; } = new List<Character>();
        public List<WorldEntityField> DynamicFields { get; set; } = new List<WorldEntityField>();
    }
}

