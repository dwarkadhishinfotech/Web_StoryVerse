using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Accent",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Accessories",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AppearanceNotes",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Build",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Complexion",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HairStyle",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredColors",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpeechPattern",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VoiceTone",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accent",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Accessories",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "AppearanceNotes",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Build",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Complexion",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "HairStyle",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "PreferredColors",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "SpeechPattern",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "VoiceTone",
                table: "DI_TRN_WebCharacters");
        }
    }
}
