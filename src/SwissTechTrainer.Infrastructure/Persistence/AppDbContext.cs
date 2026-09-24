using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserCategoryProgress> UserCategoryProgresses => Set<UserCategoryProgress>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile
        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).IsRequired().HasMaxLength(100);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);
            b.Property(u => u.TargetRole).HasMaxLength(200);

            b.HasMany(u => u.Progresses)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.Submissions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserCategoryProgress
        modelBuilder.Entity<UserCategoryProgress>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasIndex(p => new { p.UserId, p.Category }).IsUnique();
        });

        // Exercise
        modelBuilder.Entity<Exercise>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Title).IsRequired().HasMaxLength(300);
            b.Property(e => e.ExpectedOutputFormat).HasMaxLength(200);
            b.HasIndex(e => new { e.Category, e.Level });

            b.HasMany(e => e.Submissions)
                .WithOne(s => s.Exercise)
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Submission
        modelBuilder.Entity<Submission>(b =>
        {
            b.HasKey(s => s.Id);

            b.HasOne(s => s.Evaluation)
                .WithOne(e => e.Submission)
                .HasForeignKey<Evaluation>(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Evaluation
        modelBuilder.Entity<Evaluation>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.LlmModelUsed).HasMaxLength(100);

            // Store CriteriaScores and CodeSuggestions as JSON
            b.Property(e => e.CriteriaScores)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<CriterionScore>>(v, JsonOptions) ?? new List<CriterionScore>()
                );

            b.Property(e => e.CodeSuggestions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<CodeDiffSnippet>>(v, JsonOptions) ?? new List<CodeDiffSnippet>()
                );
        });
    }
}
