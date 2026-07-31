using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Age",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Alignment",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Allies",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArcType",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundSummary",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Backstory",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClothingStyle",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desires",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DistinguishingFeatures",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Enemies",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EyeColor",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fears",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Flaws",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HairColor",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Height",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyEvents",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoveInterests",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nicknames",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OneLineDescription",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonalityTraits",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Strengths",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "DI_TRN_WebCharacters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DI_TRN_WebCharacters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Alignment",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Allies",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "ArcType",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "BackgroundSummary",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Backstory",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "ClothingStyle",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Desires",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "DistinguishingFeatures",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Enemies",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "EyeColor",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Fears",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Flaws",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "HairColor",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "KeyEvents",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "LoveInterests",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Nicknames",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "OneLineDescription",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "PersonalityTraits",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Strengths",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "DI_TRN_WebCharacters");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DI_TRN_WebCharacters");
        }
    }
}
