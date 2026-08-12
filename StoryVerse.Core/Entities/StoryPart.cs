using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    public class StoryPart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public string Title { get; set; } = string.Empty; // e.g. "PART I - The Beginning"
        public int Order { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
