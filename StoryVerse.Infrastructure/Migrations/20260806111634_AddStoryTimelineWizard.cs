using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryTimelineWizard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DI_TRN_StoryTimelines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimelineType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalendarStartDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultTimelineView = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventGrouping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowTimeOnTimeline = table.Column<bool>(type: "bit", nullable: false),
                    ShowEventIcons = table.Column<bool>(type: "bit", nullable: false),
                    ShowEventDescriptions = table.Column<bool>(type: "bit", nullable: false),
                    CompactMode = table.Column<bool>(type: "bit", nullable: false),
                    AllowOverlappingEvents = table.Column<bool>(type: "bit", nullable: false),
                    EnableTimelineDependencies = table.Column<bool>(type: "bit", nullable: false),
                    AutoSortNewEvents = table.Column<bool>(type: "bit", nullable: false),
                    EnableReminders = table.Column<bool>(type: "bit", nullable: false),
                    LockTimelineDates = table.Column<bool>(type: "bit", nullable: false),
                    ShowFutureEvents = table.Column<bool>(type: "bit", nullable: false),
                    ShowCompletedEvents = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_StoryTimelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryTimelines_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineStoryArcs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryTimelineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryArcId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineStoryArcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineStoryArcs_DI_TRN_StoryArcs_StoryArcId",
                        column: x => x.StoryArcId,
                        principalTable: "DI_TRN_StoryArcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineStoryArcs_DI_TRN_StoryTimelines_StoryTimelineId",
                        column: x => x.StoryTimelineId,
                        principalTable: "DI_TRN_StoryTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_StoryTimelines_StoryId",
                table: "DI_TRN_StoryTimelines",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineStoryArcs_StoryArcId",
                table: "DI_TRN_TimelineStoryArcs",
                column: "StoryArcId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineStoryArcs_StoryTimelineId",
                table: "DI_TRN_TimelineStoryArcs",
                column: "StoryTimelineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineStoryArcs");

            migrationBuilder.DropTable(
                name: "DI_TRN_StoryTimelines");
        }
    }
}
