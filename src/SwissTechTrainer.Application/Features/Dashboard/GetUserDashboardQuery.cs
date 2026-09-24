using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Dashboard;

/// <summary>
/// Query to retrieve the candidate's complete dashboard metrics, category progression, and mastery percentages.
/// </summary>
/// <param name="UserId">Optional user ID filter, defaulting to current ambient user.</param>
public sealed record GetUserDashboardQuery(Guid? UserId = null) : IRequest<UserDashboardDto>;

/// <summary>
/// Handles retrieving or initializing candidate profile and calculating multi-dimensional category mastery.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class GetUserDashboardQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetUserDashboardQuery, UserDashboardDto>
{
    /// <summary>
    /// Executes the query to compute category mastery metrics and overall readiness percentage.
    /// </summary>
    /// <param name="request">The dashboard query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="UserDashboardDto"/>.</returns>
    public async Task<UserDashboardDto> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var user = await context.UserProfiles
            .Include(u => u.Progresses)
            .Include(u => u.Submissions)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user == null)
        {
            user = UserProfile.Create(currentUserService.Username, "candidate@swisstech.ch");
            context.UserProfiles.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        var allCategoryTypes = Enum.GetValues<CategoryType>();

        var categories = (from cat in allCategoryTypes
            let progress = user.GetOrCreateProgress(cat)
            let progressPercent = Math.Min(100.0, progress.CompletedLevelsCount / 5.0 * 100.0)
            select new DashboardCategoryDto
            {
                Category = cat,
                DisplayName = cat.GetDisplayName(),
                ShortDescription = cat.GetShortDescription(),
                Icon = cat.GetIcon(),
                CurrentLevel = progress.CurrentLevel,
                CurrentLevelLabel = progress.CurrentLevel.GetLabel(),
                CompletedLevelsCount = progress.CompletedLevelsCount,
                ProgressPercentage = Math.Round(progressPercent, 1),
                HighestScoreAchieved = Math.Round(progress.HighestScoreAchieved, 1),
                ConsecutiveFailures = progress.ConsecutiveFailures,
                HintModeActive = progress.HintModeActive,
                LastAttemptAt = progress.LastAttemptAt
            }).ToList();

        var totalCompleted = user.Progresses.Sum(p => p.CompletedLevelsCount);
        var overallMastery = (double)totalCompleted / (allCategoryTypes.Length * 5) * 100.0;

        return new UserDashboardDto
        {
            UserId = user.Id,
            Username = user.Username,
            TargetRole = user.TargetRole,
            TotalCompletedExercises = totalCompleted,
            OverallMasteryPercentage = Math.Round(overallMastery, 1),
            Categories = categories
        };
    }
}
