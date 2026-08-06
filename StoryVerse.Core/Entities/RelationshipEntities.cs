using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    public class CharacterRelationship
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public Guid SourceCharacterId { get; set; }
        public Character SourceCharacter { get; set; } = null!;

        public Guid TargetCharacterId { get; set; }
        public Character TargetCharacter { get; set; } = null!;

        // Relationship Type: Friend, Enemy, Sibling, Parent, Child, Teacher, Student, Partner, Married, Business Partner, Mentor, Custom
        public string RelationshipType { get; set; } = "Friend";
        public string? Notes { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CharacterWorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid WorldEntityId { get; set; }
        public WorldEntity WorldEntity { get; set; } = null!;

        // Relationship Type: Lives In, Born In, Works At, Member Of, Protects, Studies At, Owns, Belongs To, Custom
        public string RelationshipType { get; set; } = "Member Of";
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ChapterCharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public string Role { get; set; } = "Major"; // Major, Minor, Cameo, Mentioned
        public string? Notes { get; set; }
    }

    public class ChapterWorldEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        public Guid WorldEntityId { get; set; }
        public WorldEntity WorldEntity { get; set; } = null!;

        public string? Notes { get; set; }
    }
}
