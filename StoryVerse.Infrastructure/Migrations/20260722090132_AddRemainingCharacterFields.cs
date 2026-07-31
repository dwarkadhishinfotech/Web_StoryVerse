using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingCharacterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorNotes",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundDocumentUrl",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentResidence",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomDocumentUrl",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FamilyBackground",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FamilyCrest",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipChartUrl",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipsJson",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SocioeconomicStatus",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Upbringing",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorNotes",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "BackgroundDocumentUrl",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "CurrentResidence",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "CustomDocumentUrl",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "FamilyBackground",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "FamilyCrest",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "RelationshipChartUrl",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "RelationshipsJson",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "SocioeconomicStatus",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Upbringing",
                table: "DI_TRN_WebCharacters");
        }
    }
}
