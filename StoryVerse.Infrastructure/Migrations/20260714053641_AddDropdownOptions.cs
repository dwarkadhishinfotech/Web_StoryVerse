using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDropdownOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DI_MST_DropdownOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_MST_DropdownOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DI_MST_DropdownOptions_Category",
                table: "DI_MST_DropdownOptions",
                column: "Category");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetDropdownOptionsByCategory
                    @Category NVARCHAR(450)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT * FROM DI_MST_DropdownOptions
                    WHERE Category = @Category AND IsActive = 1
                    ORDER BY DisplayOrder;
                END
            ");

            migrationBuilder.Sql(@"
                INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive) VALUES
                (NEWID(), 'StoryType', 'Novel', 'Novel', 'A long fictional narrative.', 1, 1),
                (NEWID(), 'StoryType', 'ShortStory', 'Short Story', 'A brief fictional prose narrative.', 2, 1),
                (NEWID(), 'StoryType', 'Novella', 'Novella', 'A short novel or long short story.', 3, 1),
                (NEWID(), 'ProjectStatus', 'Planning', 'Planning', 'Currently brainstorming and outlining.', 1, 1),
                (NEWID(), 'ProjectStatus', 'Drafting', 'Drafting', 'Actively writing the first draft.', 2, 1),
                (NEWID(), 'ProjectStatus', 'Editing', 'Editing', 'Revising and polishing the manuscript.', 3, 1),
                (NEWID(), 'TargetAudience', 'YoungAdult', 'Young Adult', 'Aimed at readers aged 12-18.', 1, 1),
                (NEWID(), 'TargetAudience', 'Adult', 'Adult', 'Aimed at mature readers.', 2, 1),
                (NEWID(), 'Language', 'English', 'English', 'English language.', 1, 1),
                (NEWID(), 'Language', 'Spanish', 'Spanish', 'Spanish language.', 2, 1)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DI_MST_DropdownOptions");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetDropdownOptionsByCategory");
        }
    }
}
