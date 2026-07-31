using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Motivations",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonalityOverview",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Temperament",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValuesBeliefs",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motivations",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "PersonalityOverview",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Temperament",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "ValuesBeliefs",
                table: "DI_TRN_WebCharacters");
        }
    }
}
