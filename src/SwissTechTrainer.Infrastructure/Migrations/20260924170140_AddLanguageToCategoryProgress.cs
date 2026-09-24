using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissTechTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageToCategoryProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StarterCode = table.Column<string>(type: "TEXT", nullable: false),
                    ExpectedOutputFormat = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Hints = table.Column<string>(type: "TEXT", nullable: false),
                    PreviousExerciseReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsAIGenerated = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetRole = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedCode = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "TEXT", nullable: false),
                    SubmissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCategoryProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedLevelsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    HighestScoreAchieved = table.Column<double>(type: "REAL", nullable: false),
                    HintModeActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryProgresses_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DeterministicScore = table.Column<double>(type: "REAL", nullable: false),
                    PassedThreshold = table.Column<bool>(type: "INTEGER", nullable: false),
                    GeneralFeedback = table.Column<string>(type: "TEXT", nullable: false),
                    LlmModelUsed = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProcessingDurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CriteriaScores = table.Column<string>(type: "TEXT", nullable: false),
                    CodeSuggestions = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_SubmissionId",
                table: "Evaluations",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Category_Level_Language",
                table: "Exercises",
                columns: new[] { "Category", "Level", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ExerciseId",
                table: "Submissions",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryProgresses_UserId_Category_Language",
                table: "UserCategoryProgresses",
                columns: new[] { "UserId", "Category", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "UserCategoryProgresses");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
