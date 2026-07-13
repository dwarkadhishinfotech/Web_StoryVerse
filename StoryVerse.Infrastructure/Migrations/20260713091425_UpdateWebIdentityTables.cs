using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWebIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create DI_MST_AspNetRoles
            migrationBuilder.CreateTable(
                name: "DI_MST_AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_MST_AspNetRoles", x => x.Id);
                });

            // 2. Create DI_MST_AspNetUsers
            migrationBuilder.CreateTable(
                name: "DI_MST_AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_MST_AspNetUsers", x => x.Id);
                });

            // 3. Create DI_TRN_AspNetRoleClaims
            migrationBuilder.CreateTable(
                name: "DI_TRN_AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetRoleClaims_DI_MST_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DI_MST_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 4. Create DI_TRN_AspNetUserClaims
            migrationBuilder.CreateTable(
                name: "DI_TRN_AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetUserClaims_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 5. Create DI_TRN_AspNetUserLogins
            migrationBuilder.CreateTable(
                name: "DI_TRN_AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetUserLogins_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 6. Create DI_TRN_AspNetUserRoles
            migrationBuilder.CreateTable(
                name: "DI_TRN_AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetUserRoles_DI_MST_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DI_MST_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetUserRoles_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 7. Create DI_TRN_AspNetUserTokens
            migrationBuilder.CreateTable(
                name: "DI_TRN_AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_DI_TRN_AspNetUserTokens_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 8. Create DI_TRN_ActivityLogs
            migrationBuilder.CreateTable(
                name: "DI_TRN_ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_ActivityLogs_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 9. Create DI_TRN_WebStories
            migrationBuilder.CreateTable(
                name: "DI_TRN_WebStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetWordCount = table.Column<int>(type: "int", nullable: false),
                    CurrentWordCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_WebStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_WebStories_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 10. Create DI_TRN_UserGoals
            migrationBuilder.CreateTable(
                name: "DI_TRN_UserGoals",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DailyWordCountGoal = table.Column<int>(type: "int", nullable: false),
                    WordsWrittenToday = table.Column<int>(type: "int", nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_UserGoals", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_DI_TRN_UserGoals_DI_MST_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DI_MST_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 11. Create DI_TRN_WebChapters
            migrationBuilder.CreateTable(
                name: "DI_TRN_WebChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_WebChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_WebChapters_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 12. Create DI_TRN_WebCharacters
            migrationBuilder.CreateTable(
                name: "DI_TRN_WebCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_TRN_WebCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_TRN_WebCharacters_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 13. Create DI_MST_WebLocations
            migrationBuilder.CreateTable(
                name: "DI_MST_WebLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DI_MST_WebLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DI_MST_WebLocations_DI_TRN_WebStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "DI_TRN_WebStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Indexes
            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AspNetRoleClaims_RoleId",
                table: "DI_TRN_AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "DI_MST_AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AspNetUserClaims_UserId",
                table: "DI_TRN_AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AspNetUserLogins_UserId",
                table: "DI_TRN_AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_AspNetUserRoles_RoleId",
                table: "DI_TRN_AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "DI_MST_AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "DI_MST_AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_ActivityLogs_UserId",
                table: "DI_TRN_ActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_WebStories_UserId",
                table: "DI_TRN_WebStories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_WebChapters_StoryId",
                table: "DI_TRN_WebChapters",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_TRN_WebCharacters_StoryId",
                table: "DI_TRN_WebCharacters",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DI_MST_WebLocations_StoryId",
                table: "DI_MST_WebLocations",
                column: "StoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DI_MST_WebLocations");

            migrationBuilder.DropTable(
                name: "DI_TRN_WebCharacters");

            migrationBuilder.DropTable(
                name: "DI_TRN_WebChapters");

            migrationBuilder.DropTable(
                name: "DI_TRN_UserGoals");

            migrationBuilder.DropTable(
                name: "DI_TRN_WebStories");

            migrationBuilder.DropTable(
                name: "DI_TRN_ActivityLogs");

            migrationBuilder.DropTable(
                name: "DI_TRN_AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DI_TRN_AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "DI_TRN_AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "DI_TRN_AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "DI_TRN_AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "DI_MST_AspNetUsers");

            migrationBuilder.DropTable(
                name: "DI_MST_AspNetRoles");
        }
    }
}
