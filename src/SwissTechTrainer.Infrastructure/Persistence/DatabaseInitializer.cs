using Microsoft.EntityFrameworkCore;

namespace SwissTechTrainer.Infrastructure.Persistence;

/// <summary>
/// Handles database schema initialization, migrations, and backwards-compatible schema upgrades across supported relational providers.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the database by applying pending migrations or synchronizing legacy schemas.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task InitializeAsync(AppDbContext context, CancellationToken ct = default)
    {
        if (context.Database.IsNpgsql())
        {
            await InitializePostgresAsync(context, ct);
        }
        else if (context.Database.IsSqlite())
        {
            await InitializeSqliteAsync(context, ct);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(ct);
        }
    }

    private static async Task InitializePostgresAsync(AppDbContext context, CancellationToken ct)
    {
        // Check if database exists and whether it contains tables from a pre-migration EnsureCreated setup
        const string checkAndUpgradeSql = """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                      AND (table_name = 'UserCategoryProgresses' OR table_name = 'usercategoryprogresses')
                ) THEN
                    -- Upgrade existing tables with Language column if missing
                    ALTER TABLE "UserCategoryProgresses" ADD COLUMN IF NOT EXISTS "Language" integer NOT NULL DEFAULT 1;
                    ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "Language" integer NOT NULL DEFAULT 1;
                    ALTER TABLE "Submissions" ADD COLUMN IF NOT EXISTS "Language" integer NOT NULL DEFAULT 1;
                    ALTER TABLE "Evaluations" ADD COLUMN IF NOT EXISTS "Language" integer NOT NULL DEFAULT 1;

                    -- Update indexes to include Language
                    DROP INDEX IF EXISTS "IX_UserCategoryProgresses_UserId_Category";
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserCategoryProgresses_UserId_Category_Language" 
                        ON "UserCategoryProgresses" ("UserId", "Category", "Language");

                    DROP INDEX IF EXISTS "IX_Exercises_Category_Level";
                    CREATE INDEX IF NOT EXISTS "IX_Exercises_Category_Level_Language" 
                        ON "Exercises" ("Category", "Level", "Language");

                    -- Ensure EF Migrations history table exists and registers the initial migration
                    CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                        "MigrationId" character varying(150) NOT NULL,
                        "ProductVersion" character varying(32) NOT NULL,
                        CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                    );

                    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('20260924170140_AddLanguageToCategoryProgress', '10.0.0')
                    ON CONFLICT DO NOTHING;
                END IF;
            END $$;
            """;

        try
        {
            await context.Database.ExecuteSqlRawAsync(checkAndUpgradeSql, ct);
            await context.Database.MigrateAsync(ct);
        }
        catch
        {
            // Fallback to EnsureCreated if migrations cannot be applied
            await context.Database.EnsureCreatedAsync(ct);
        }
    }

    private static async Task InitializeSqliteAsync(AppDbContext context, CancellationToken ct)
    {
        try
        {
            // For SQLite, if tables already exist without Language column, add them before migrating
            using var connection = context.Database.GetDbConnection();
            await connection.OpenAsync(ct);

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UserCategoryProgresses';";
                var tableExists = await cmd.ExecuteScalarAsync(ct) != null;

                if (tableExists)
                {
                    // Check if Language column exists
                    cmd.CommandText = "PRAGMA table_info(UserCategoryProgresses);";
                    bool hasLanguage = false;
                    using (var reader = await cmd.ExecuteReaderAsync(ct))
                    {
                        while (await reader.ReadAsync(ct))
                        {
                            if (string.Equals(reader.GetString(1), "Language", StringComparison.OrdinalIgnoreCase))
                            {
                                hasLanguage = true;
                                break;
                            }
                        }
                    }

                    if (!hasLanguage)
                    {
                        cmd.CommandText = """
                            ALTER TABLE "UserCategoryProgresses" ADD COLUMN "Language" INTEGER NOT NULL DEFAULT 1;
                            ALTER TABLE "Exercises" ADD COLUMN "Language" INTEGER NOT NULL DEFAULT 1;
                            ALTER TABLE "Submissions" ADD COLUMN "Language" INTEGER NOT NULL DEFAULT 1;
                            ALTER TABLE "Evaluations" ADD COLUMN "Language" INTEGER NOT NULL DEFAULT 1;
                            CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserCategoryProgresses_UserId_Category_Language" ON "UserCategoryProgresses" ("UserId", "Category", "Language");
                            CREATE INDEX IF NOT EXISTS "IX_Exercises_Category_Level_Language" ON "Exercises" ("Category", "Level", "Language");
                            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                                "ProductVersion" TEXT NOT NULL
                            );
                            INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                            VALUES ('20260924170140_AddLanguageToCategoryProgress', '10.0.0');
                            """;
                        await cmd.ExecuteNonQueryAsync(ct);
                    }
                }
            }

            await context.Database.MigrateAsync(ct);
        }
        catch
        {
            await context.Database.EnsureCreatedAsync(ct);
        }
    }
}
