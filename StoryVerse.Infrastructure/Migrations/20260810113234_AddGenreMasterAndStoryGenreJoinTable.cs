using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreMasterAndStoryGenreJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DI_MST_Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_MST_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_StoryGenres",
                columns: table => new
                {
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_StoryGenres", x => new { x.StoryId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryGenres_DI_MST_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "DI_MST_Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryGenres_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DI_MST_Genres",
                columns: new[] { "Id", "CreatedAt", "Description", "Icon", "IsActive", "Name", "Slug", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Magic, mythical creatures, and imaginary worlds.", "castle", true, "Fantasy", "fantasy", 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Futuristic, space, technology, and advanced worlds.", "rocket", true, "Science Fiction", "science-fiction", 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Puzzles, crime, secrets, and investigations.", "search", true, "Mystery", "mystery", 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Love, relationships, and emotional journeys.", "heart", true, "Romance", "romance", 4 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suspense, tension, and edge of the seat.", "theater", true, "Thriller", "thriller", 5 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stories set in the past with real or imagined events.", "landmark", true, "Historical Fiction", "historical-fiction", 6 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dark, eerie, and supernatural themes.", "ghost", true, "Horror", "horror", 7 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Humor, light-hearted and feel-good stories.", "smile", true, "Comedy", "comedy", 8 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Exciting journeys, quests, and daring challenges.", "compass", true, "Adventure", "adventure", 9 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Character-driven stories with emotional depth.", "clapperboard", true, "Drama", "drama", 10 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My genre is not listed here.", "plus", true, "Other", "other", 11 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DI_MST_Genres_Slug",
                table: "DI_MST_Genres",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_StoryGenres_GenreId",
                table: "DI_TRN_StoryGenres",
                column: "GenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DI_TRN_StoryGenres");

            migrationBuilder.DropTable(
                name: "DI_MST_Genres");
        }
    }
}
