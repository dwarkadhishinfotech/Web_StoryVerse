using System;
using System.Collections.Generic;

namespace StoryVerse.Core.Entities
{
    /// <summary>
    /// Master list of genres (seeded, controlled data).
    /// Table: DI_MST_Genres
    /// </summary>
    public class Genre
    {
        public int Id { get; set; }

        /// <summary>Display name e.g. "Science Fiction"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>URL-safe slug e.g. "science-fiction"</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>Lucide icon name e.g. "rocket"</summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>Short description shown on genre cards</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Controls display order on the genre picker</summary>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<StoryGenre> StoryGenres { get; set; } = new List<StoryGenre>();
    }

    /// <summary>
    /// Join table linking Stories ↔ Genres (many-to-many).
    /// Table: DI_TRN_StoryGenres
    /// </summary>
    public class StoryGenre
    {
        public Guid StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;

        /// <summary>True if this is the primary/main genre of the story</summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>Preserves the order in which the user selected genres</summary>
        public int SortOrder { get; set; } = 0;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
