using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsDropdownOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive) VALUES
                (NEWID(), 'WritingStyle', 'Descriptive', 'Descriptive', 'Rich, detailed language painting vivid pictures.', 1, 1),
                (NEWID(), 'WritingStyle', 'Concise', 'Concise', 'Clear, brief, and to the point.', 2, 1),
                (NEWID(), 'WritingStyle', 'ActionPacked', 'Action-Packed', 'Fast-paced, high energy writing.', 3, 1),
                (NEWID(), 'WritingStyle', 'Poetic', 'Poetic', 'Lyrical and highly stylized prose.', 4, 1),
                (NEWID(), 'PointOfView', 'FirstPerson', 'First Person', 'Told from the perspective of ''I''.', 1, 1),
                (NEWID(), 'PointOfView', 'SecondPerson', 'Second Person', 'Told from the perspective of ''You''.', 2, 1),
                (NEWID(), 'PointOfView', 'ThirdPersonLimited', 'Third Person Limited', 'Told from an outside perspective, focusing on one character.', 3, 1),
                (NEWID(), 'PointOfView', 'ThirdPersonOmniscient', 'Third Person Omniscient', 'An all-knowing outside perspective.', 4, 1),
                (NEWID(), 'Tense', 'Past', 'Past Tense', 'Events that have already happened.', 1, 1),
                (NEWID(), 'Tense', 'Present', 'Present Tense', 'Events happening right now.', 2, 1),
                (NEWID(), 'Tense', 'Future', 'Future Tense', 'Events that will happen.', 3, 1)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
