using System;

namespace StoryVerse.Core.Entities
{
    public class DropdownOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Category { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
