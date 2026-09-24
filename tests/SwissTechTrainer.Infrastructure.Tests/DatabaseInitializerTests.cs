using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;
using SwissTechTrainer.Infrastructure.Persistence;
using Xunit;

namespace SwissTechTrainer.Infrastructure.Tests;

public class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializeAsync_OnFreshDatabase_CreatesSchemaAndRunsMigrations()
    {
        string dbName = $"FreshDb_{Guid.NewGuid():N}.db";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbName}")
            .Options;

        try
        {
            await using var context = new AppDbContext(options);
            await DatabaseInitializer.InitializeAsync(context);

            // Verify tables and seeding works
            await DatabaseSeeder.SeedAsync(context);

            var user = await context.UserProfiles.Include(u => u.Progresses).FirstOrDefaultAsync();
            user.Should().NotBeNull();
            user!.Progresses.Should().NotBeEmpty();
            user.Progresses.Should().Contain(p => p.Language == ProgrammingLanguage.CSharp);
        }
        finally
        {
            await using var cleanupContext = new AppDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenLegacyTablesExistWithoutLanguageColumn_UpgradesSchemaGracefully()
    {
        string dbName = $"LegacyDb_{Guid.NewGuid():N}.db";
        var connectionString = $"Data Source={dbName}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        try
        {
            // 1. Manually simulate legacy database schema before multi-language support (no Language column)
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = """
                    CREATE TABLE "UserProfiles" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_UserProfiles" PRIMARY KEY,
                        "Username" TEXT NOT NULL,
                        "Email" TEXT NOT NULL,
                        "TargetRole" TEXT NOT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "LastActiveAt" TEXT NOT NULL
                    );
                    CREATE TABLE "UserCategoryProgresses" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_UserCategoryProgresses" PRIMARY KEY,
                        "UserId" TEXT NOT NULL,
                        "Category" INTEGER NOT NULL,
                        "CurrentLevel" INTEGER NOT NULL,
                        "ConsecutiveFailures" INTEGER NOT NULL,
                        "CompletedLevelsCount" INTEGER NOT NULL,
                        "HighestScoreAchieved" REAL NOT NULL,
                        "HintModeActive" INTEGER NOT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "LastAttemptAt" TEXT NULL,
                        CONSTRAINT "FK_UserCategoryProgresses_UserProfiles_UserId" FOREIGN KEY ("UserId") REFERENCES "UserProfiles" ("Id") ON DELETE CASCADE
                    );
                    CREATE TABLE "Exercises" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_Exercises" PRIMARY KEY,
                        "Category" INTEGER NOT NULL,
                        "Level" INTEGER NOT NULL,
                        "Title" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "StarterCode" TEXT NOT NULL,
                        "ExpectedOutputFormat" TEXT NOT NULL,
                        "Hints" TEXT NOT NULL,
                        "PreviousExerciseReferenceId" TEXT NULL,
                        "IsAIGenerated" INTEGER NOT NULL,
                        "CreatedAt" TEXT NOT NULL
                    );
                    CREATE TABLE "Submissions" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_Submissions" PRIMARY KEY,
                        "ExerciseId" TEXT NOT NULL,
                        "UserId" TEXT NOT NULL,
                        "SubmittedCode" TEXT NOT NULL,
                        "AdditionalNotes" TEXT NOT NULL,
                        "SubmissionType" INTEGER NOT NULL,
                        "SubmittedAt" TEXT NOT NULL
                    );
                    CREATE TABLE "Evaluations" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_Evaluations" PRIMARY KEY,
                        "SubmissionId" TEXT NOT NULL,
                        "Status" INTEGER NOT NULL,
                        "DeterministicScore" REAL NOT NULL,
                        "PassedThreshold" INTEGER NOT NULL,
                        "GeneralFeedback" TEXT NOT NULL,
                        "LlmModelUsed" TEXT NOT NULL,
                        "ProcessingDurationMs" INTEGER NOT NULL,
                        "EvaluatedAt" TEXT NOT NULL,
                        "CriteriaScores" TEXT NOT NULL,
                        "CodeSuggestions" TEXT NOT NULL
                    );
                    INSERT INTO "UserProfiles" ("Id", "Username", "Email", "TargetRole", "CreatedAt", "LastActiveAt")
                    VALUES ('11111111-1111-1111-1111-111111111111', 'LegacyUser', 'legacy@swisstech.ch', 'Dev', '2026-01-01', '2026-01-01');
                    INSERT INTO "UserCategoryProgresses" ("Id", "UserId", "Category", "CurrentLevel", "ConsecutiveFailures", "CompletedLevelsCount", "HighestScoreAchieved", "HintModeActive", "CreatedAt")
                    VALUES ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 1, 1, 0, 0, 0.0, 0, '2026-01-01');
                    """;
                await cmd.ExecuteNonQueryAsync();
            }

            // 2. Run DatabaseInitializer to upgrade the legacy database
            await using (var context = new AppDbContext(options))
            {
                await DatabaseInitializer.InitializeAsync(context);

                // 3. Verify seeding and querying work without "column Language does not exist" error
                await DatabaseSeeder.SeedAsync(context);

                var user = await context.UserProfiles.Include(u => u.Progresses).FirstOrDefaultAsync();
                user.Should().NotBeNull();
                user!.Progresses.Should().NotBeEmpty();

                var csharpProg = user.Progresses.First(p => p.Category == CategoryType.Coding && p.Language == ProgrammingLanguage.CSharp);
                csharpProg.Should().NotBeNull();
                csharpProg.Language.Should().Be(ProgrammingLanguage.CSharp);
            }
        }
        finally
        {
            await using var cleanupContext = new AppDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task Evaluation_WithCriteriaScoresAndCodeSuggestions_PersistsAndLoadsProperly()
    {
        string dbName = $"EvalDb_{Guid.NewGuid():N}.db";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbName}")
            .Options;

        try
        {
            await using var context = new AppDbContext(options);
            await DatabaseInitializer.InitializeAsync(context);

            var exercise = Exercise.Create(CategoryType.Coding, DifficultyLevel.Level1, "Test Ex", "Desc", "Starter");
            var user = UserProfile.Create("EvalUser", "eval@swiss.ch");
            var submission = Submission.Create(exercise.Id, user.Id, "code");
            var evaluation = Evaluation.CreatePending(submission.Id);
            evaluation.Complete(
                [new CriterionScore("Correctness", 40.0, 95.0, "Evidence", "Rationale")],
                [new CodeDiffSnippet("Refactor", "int x=1;", "const int X=1;", "Clean Code")],
                "Good job",
                "MockLlm",
                120);

            context.Exercises.Add(exercise);
            context.UserProfiles.Add(user);
            context.Submissions.Add(submission);
            context.Evaluations.Add(evaluation);
            await context.SaveChangesAsync();

            // Reload and verify
            await using var verifyContext = new AppDbContext(options);
            var loadedEval = await verifyContext.Evaluations.FirstOrDefaultAsync(e => e.Id == evaluation.Id);
            loadedEval.Should().NotBeNull();
            loadedEval!.CriteriaScores.Should().HaveCount(1);
            loadedEval.CriteriaScores.First().Name.Should().Be("Correctness");
            loadedEval.CodeSuggestions.Should().HaveCount(1);
            loadedEval.CodeSuggestions.First().Title.Should().Be("Refactor");
        }
        finally
        {
            await using var cleanupContext = new AppDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}
