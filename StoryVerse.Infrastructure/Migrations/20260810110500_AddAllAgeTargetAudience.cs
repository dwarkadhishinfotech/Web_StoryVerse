using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllAgeTargetAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DI_MST_DropdownOptions WHERE Category = 'TargetAudience' AND Value = 'AllAgeAudience')
                BEGIN
                    UPDATE DI_MST_DropdownOptions SET DisplayOrder = DisplayOrder + 1 WHERE Category = 'TargetAudience';

                    INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive) VALUES
                    (NEWID(), 'TargetAudience', 'AllAgeAudience', 'All Age Audience', 'Aimed at readers of all age groups.', 1, 1);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM DI_MST_DropdownOptions WHERE Category = 'TargetAudience' AND Value = 'AllAgeAudience';
            ");
        }
    }
}
