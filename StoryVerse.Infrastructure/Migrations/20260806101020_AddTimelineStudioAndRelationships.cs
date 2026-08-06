using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimelineStudioAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += N'ALTER TABLE ' + QUOTENAME(CONSTRAINT_SCHEMA) + N'.' + QUOTENAME(TABLE_NAME) + N' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + N';'
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                WHERE CONSTRAINT_TYPE = 'FOREIGN KEY' AND (TABLE_NAME LIKE 'DI_TRN_%' OR TABLE_NAME = 'DI_TRN_WebStories');
                IF @sql <> N'' EXEC sp_executesql @sql;

                IF OBJECT_ID(N'dbo.DI_TRN_StoryArcEvents', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_StoryArcEvents;
                IF OBJECT_ID(N'dbo.DI_TRN_StoryArcs', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_StoryArcs;
                IF OBJECT_ID(N'dbo.DI_TRN_CharacterRelationships', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_CharacterRelationships;
                IF OBJECT_ID(N'dbo.DI_TRN_CharacterWorldEntities', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_CharacterWorldEntities;
                IF OBJECT_ID(N'dbo.DI_TRN_ChapterCharacters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ChapterCharacters;
                IF OBJECT_ID(N'dbo.DI_TRN_ChapterWorldEntities', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ChapterWorldEntities;
                IF OBJECT_ID(N'dbo.DI_TRN_TimelineEventChapters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_TimelineEventChapters;
                IF OBJECT_ID(N'dbo.DI_TRN_TimelineRelationships', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_TimelineRelationships;
                IF OBJECT_ID(N'dbo.DI_TRN_TimelineWorldEntities', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_TimelineWorldEntities;
                IF OBJECT_ID(N'dbo.DI_TRN_TimelineCharacters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_TimelineCharacters;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchAssets', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchAssets;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchChapters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchChapters;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchTimelineEvents', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchTimelineEvents;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchWorldEntities', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchWorldEntities;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchCharacters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchCharacters;
                IF OBJECT_ID(N'dbo.DI_TRN_ResearchNotes', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_ResearchNotes;
                IF OBJECT_ID(N'dbo.DI_TRN_AssetResearchNotes', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_AssetResearchNotes;
                IF OBJECT_ID(N'dbo.DI_TRN_AssetChapters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_AssetChapters;
                IF OBJECT_ID(N'dbo.DI_TRN_AssetTimelineEvents', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_AssetTimelineEvents;
                IF OBJECT_ID(N'dbo.DI_TRN_AssetWorldEntities', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_AssetWorldEntities;
                IF OBJECT_ID(N'dbo.DI_TRN_AssetCharacters', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_AssetCharacters;
                IF OBJECT_ID(N'dbo.DI_TRN_Assets', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_Assets;
                IF OBJECT_ID(N'dbo.DI_TRN_TimelineEvents', N'U') IS NOT NULL DROP TABLE dbo.DI_TRN_TimelineEvents;

                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'BranchName' AND Object_ID = Object_ID(N'dbo.DI_TRN_WebStories')) ALTER TABLE dbo.DI_TRN_WebStories DROP COLUMN BranchName;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsBranch' AND Object_ID = Object_ID(N'dbo.DI_TRN_WebStories')) ALTER TABLE dbo.DI_TRN_WebStories DROP COLUMN IsBranch;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ParentStoryId' AND Object_ID = Object_ID(N'dbo.DI_TRN_WebStories')) ALTER TABLE dbo.DI_TRN_WebStories DROP COLUMN ParentStoryId;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SeriesId' AND Object_ID = Object_ID(N'dbo.DI_TRN_WebStories')) ALTER TABLE dbo.DI_TRN_WebStories DROP COLUMN SeriesId;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'UniverseId' AND Object_ID = Object_ID(N'dbo.DI_TRN_WebStories')) ALTER TABLE dbo.DI_TRN_WebStories DROP COLUMN UniverseId;
            ");

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                table: "DI_TRN_WebStories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBranch",
                table: "DI_TRN_WebStories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentStoryId",
                table: "DI_TRN_WebStories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeriesId",
                table: "DI_TRN_WebStories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UniverseId",
                table: "DI_TRN_WebStories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DI_TRN_Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_Assets_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ChapterCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ChapterCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ChapterCharacters_DI_TRN_WebChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "DI_TRN_WebChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ChapterCharacters_DI_TRN_WebCharacters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ChapterWorldEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ChapterWorldEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ChapterWorldEntities_DI_TRN_WebChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "DI_TRN_WebChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ChapterWorldEntities_DI_TRN_WorldEntities_WorldEntityId",
                        column: x => x.WorldEntityId,
                        principalTable: "DI_TRN_WorldEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_CharacterRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceCharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetCharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_CharacterRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_CharacterRelationships_DI_TRN_WebCharacters_SourceCharacterId",
                        column: x => x.SourceCharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DI_TRN_CharacterRelationships_DI_TRN_WebCharacters_TargetCharacterId",
                        column: x => x.TargetCharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DI_TRN_CharacterRelationships_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_CharacterWorldEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_CharacterWorldEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_CharacterWorldEntities_DI_TRN_WebCharacters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_CharacterWorldEntities_DI_TRN_WorldEntities_WorldEntityId",
                        column: x => x.WorldEntityId,
                        principalTable: "DI_TRN_WorldEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchNotes_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_StoryArcs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArcType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCompletionPercent = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_StoryArcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryArcs_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RealDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoryDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImpactNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBookmarked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineEvents_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_AssetChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AssetChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetChapters_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetChapters_DI_TRN_WebChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "DI_TRN_WebChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_AssetCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AssetCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetCharacters_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetCharacters_DI_TRN_WebCharacters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_AssetWorldEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AssetWorldEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetWorldEntities_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetWorldEntities_DI_TRN_WorldEntities_WorldEntityId",
                        column: x => x.WorldEntityId,
                        principalTable: "DI_TRN_WorldEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_AssetResearchNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AssetResearchNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetResearchNotes_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetResearchNotes_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchAssets_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchAssets_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchChapters_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchChapters_DI_TRN_WebChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "DI_TRN_WebChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchCharacters_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchCharacters_DI_TRN_WebCharacters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchWorldEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchWorldEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchWorldEntities_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchWorldEntities_DI_TRN_WorldEntities_WorldEntityId",
                        column: x => x.WorldEntityId,
                        principalTable: "DI_TRN_WorldEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_AssetTimelineEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AssetTimelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetTimelineEvents_DI_TRN_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "DI_TRN_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AssetTimelineEvents_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_ResearchTimelineEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResearchNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ResearchTimelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchTimelineEvents_DI_TRN_ResearchNotes_ResearchNoteId",
                        column: x => x.ResearchNoteId,
                        principalTable: "DI_TRN_ResearchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ResearchTimelineEvents_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_StoryArcEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryArcId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderInArc = table.Column<int>(type: "int", nullable: false),
                    ImpactLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_StoryArcEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryArcEvents_DI_TRN_StoryArcs_StoryArcId",
                        column: x => x.StoryArcId,
                        principalTable: "DI_TRN_StoryArcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_StoryArcEvents_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineCharacters_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineCharacters_DI_TRN_WebCharacters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "DI_TRN_WebCharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineEventChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineEventChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineEventChapters_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineEventChapters_DI_TRN_WebChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "DI_TRN_WebChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineRelationships_DI_TRN_TimelineEvents_SourceEventId",
                        column: x => x.SourceEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineRelationships_DI_TRN_TimelineEvents_TargetEventId",
                        column: x => x.TargetEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DI_TRN_TimelineWorldEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_TimelineWorldEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineWorldEntities_DI_TRN_TimelineEvents_TimelineEventId",
                        column: x => x.TimelineEventId,
                        principalTable: "DI_TRN_TimelineEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_TimelineWorldEntities_DI_TRN_WorldEntities_WorldEntityId",
                        column: x => x.WorldEntityId,
                        principalTable: "DI_TRN_WorldEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetChapters_AssetId",
                table: "DI_TRN_AssetChapters",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetChapters_ChapterId",
                table: "DI_TRN_AssetChapters",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetCharacters_AssetId",
                table: "DI_TRN_AssetCharacters",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetCharacters_CharacterId",
                table: "DI_TRN_AssetCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetResearchNotes_AssetId",
                table: "DI_TRN_AssetResearchNotes",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetResearchNotes_ResearchNoteId",
                table: "DI_TRN_AssetResearchNotes",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_Assets_StoryId",
                table: "DI_TRN_Assets",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetTimelineEvents_AssetId",
                table: "DI_TRN_AssetTimelineEvents",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetTimelineEvents_TimelineEventId",
                table: "DI_TRN_AssetTimelineEvents",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetWorldEntities_AssetId",
                table: "DI_TRN_AssetWorldEntities",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AssetWorldEntities_WorldEntityId",
                table: "DI_TRN_AssetWorldEntities",
                column: "WorldEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ChapterCharacters_ChapterId",
                table: "DI_TRN_ChapterCharacters",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ChapterCharacters_CharacterId",
                table: "DI_TRN_ChapterCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ChapterWorldEntities_ChapterId",
                table: "DI_TRN_ChapterWorldEntities",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ChapterWorldEntities_WorldEntityId",
                table: "DI_TRN_ChapterWorldEntities",
                column: "WorldEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_CharacterRelationships_SourceCharacterId",
                table: "DI_TRN_CharacterRelationships",
                column: "SourceCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_CharacterRelationships_StoryId",
                table: "DI_TRN_CharacterRelationships",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_CharacterRelationships_TargetCharacterId",
                table: "DI_TRN_CharacterRelationships",
                column: "TargetCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_CharacterWorldEntities_CharacterId",
                table: "DI_TRN_CharacterWorldEntities",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_CharacterWorldEntities_WorldEntityId",
                table: "DI_TRN_CharacterWorldEntities",
                column: "WorldEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchAssets_AssetId",
                table: "DI_TRN_ResearchAssets",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchAssets_ResearchNoteId",
                table: "DI_TRN_ResearchAssets",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchChapters_ChapterId",
                table: "DI_TRN_ResearchChapters",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchChapters_ResearchNoteId",
                table: "DI_TRN_ResearchChapters",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchCharacters_CharacterId",
                table: "DI_TRN_ResearchCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchCharacters_ResearchNoteId",
                table: "DI_TRN_ResearchCharacters",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchNotes_StoryId",
                table: "DI_TRN_ResearchNotes",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchTimelineEvents_ResearchNoteId",
                table: "DI_TRN_ResearchTimelineEvents",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchTimelineEvents_TimelineEventId",
                table: "DI_TRN_ResearchTimelineEvents",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchWorldEntities_ResearchNoteId",
                table: "DI_TRN_ResearchWorldEntities",
                column: "ResearchNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ResearchWorldEntities_WorldEntityId",
                table: "DI_TRN_ResearchWorldEntities",
                column: "WorldEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_StoryArcEvents_StoryArcId",
                table: "DI_TRN_StoryArcEvents",
                column: "StoryArcId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_StoryArcEvents_TimelineEventId",
                table: "DI_TRN_StoryArcEvents",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_StoryArcs_StoryId",
                table: "DI_TRN_StoryArcs",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineCharacters_CharacterId",
                table: "DI_TRN_TimelineCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineCharacters_TimelineEventId",
                table: "DI_TRN_TimelineCharacters",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineEventChapters_ChapterId",
                table: "DI_TRN_TimelineEventChapters",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineEventChapters_TimelineEventId",
                table: "DI_TRN_TimelineEventChapters",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineEvents_StoryId",
                table: "DI_TRN_TimelineEvents",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineRelationships_SourceEventId",
                table: "DI_TRN_TimelineRelationships",
                column: "SourceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineRelationships_TargetEventId",
                table: "DI_TRN_TimelineRelationships",
                column: "TargetEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineWorldEntities_TimelineEventId",
                table: "DI_TRN_TimelineWorldEntities",
                column: "TimelineEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_TimelineWorldEntities_WorldEntityId",
                table: "DI_TRN_TimelineWorldEntities",
                column: "WorldEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DI_TRN_AssetChapters");

            migrationBuilder.DropTable(
                name: "DI_TRN_AssetCharacters");

            migrationBuilder.DropTable(
                name: "DI_TRN_AssetResearchNotes");

            migrationBuilder.DropTable(
                name: "DI_TRN_AssetTimelineEvents");

            migrationBuilder.DropTable(
                name: "DI_TRN_AssetWorldEntities");

            migrationBuilder.DropTable(
                name: "DI_TRN_ChapterCharacters");

            migrationBuilder.DropTable(
                name: "DI_TRN_ChapterWorldEntities");

            migrationBuilder.DropTable(
                name: "DI_TRN_CharacterRelationships");

            migrationBuilder.DropTable(
                name: "DI_TRN_CharacterWorldEntities");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchAssets");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchChapters");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchCharacters");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchTimelineEvents");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchWorldEntities");

            migrationBuilder.DropTable(
                name: "DI_TRN_StoryArcEvents");

            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineCharacters");

            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineEventChapters");

            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineRelationships");

            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineWorldEntities");

            migrationBuilder.DropTable(
                name: "DI_TRN_Assets");

            migrationBuilder.DropTable(
                name: "DI_TRN_ResearchNotes");

            migrationBuilder.DropTable(
                name: "DI_TRN_StoryArcs");

            migrationBuilder.DropTable(
                name: "DI_TRN_TimelineEvents");

            migrationBuilder.DropColumn(
                name: "BranchName",
                table: "DI_TRN_WebStories");

            migrationBuilder.DropColumn(
                name: "IsBranch",
                table: "DI_TRN_WebStories");

            migrationBuilder.DropColumn(
                name: "ParentStoryId",
                table: "DI_TRN_WebStories");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "DI_TRN_WebStories");

            migrationBuilder.DropColumn(
                name: "UniverseId",
                table: "DI_TRN_WebStories");
        }
    }
}
