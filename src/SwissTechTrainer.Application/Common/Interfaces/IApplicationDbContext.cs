using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Domain.Entities;

namespace SwissTechTrainer.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserCategoryProgress> UserCategoryProgresses { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<Submission> Submissions { get; }
    DbSet<Evaluation> Evaluations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
