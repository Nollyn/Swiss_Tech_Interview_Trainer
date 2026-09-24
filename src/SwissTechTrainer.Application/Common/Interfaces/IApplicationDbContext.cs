using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Domain.Entities;

namespace SwissTechTrainer.Application.Common.Interfaces;

/// <summary>
/// Defines the persistence boundary and aggregate entity sets accessible to the Application layer.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the database set for user profiles.
    /// </summary>
    DbSet<UserProfile> UserProfiles { get; }

    /// <summary>
    /// Gets the database set for category progress trackers.
    /// </summary>
    DbSet<UserCategoryProgress> UserCategoryProgresses { get; }

    /// <summary>
    /// Gets the database set for generated and seed technical exercises.
    /// </summary>
    DbSet<Exercise> Exercises { get; }

    /// <summary>
    /// Gets the database set for candidate solution submissions.
    /// </summary>
    DbSet<Submission> Submissions { get; }

    /// <summary>
    /// Gets the database set for AI and deterministic evaluations.
    /// </summary>
    DbSet<Evaluation> Evaluations { get; }

    /// <summary>
    /// Persists all tracked entity modifications asynchronously to the underlying database store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
