using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Dashboard;

/// <summary>
/// Query to retrieve the candidate's complete dashboard metrics, category progression, and mastery percentages for a specific programming language.
/// </summary>
/// <param name="Language">The selected programming language focus, defaulting to C#.</param>
/// <param name="UserId">Optional user ID filter, defaulting to current ambient user.</param>
public sealed record GetUserDashboardQuery(ProgrammingLanguage Language = ProgrammingLanguage.CSharp, Guid? UserId = null) : IRequest<UserDashboardDto>;

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
        var targetLanguage = request.Language;

        var user = await context.UserProfiles
            .Include(u => u.Progresses)
            .Include(u => u.Submissions)
            .AsSplitQuery()
            .Where(u => u.Id == targetUserId)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            user = UserProfile.Create(currentUserService.Username, "candidate@swisstech.ch");
            context.UserProfiles.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        var allCategoryTypes = Enum.GetValues<CategoryType>();

        var categories = (from cat in allCategoryTypes
            let progress = user.GetOrCreateProgress(cat, targetLanguage)
            let progressPercent = Math.Min(100.0, progress.CompletedLevelsCount / 5.0 * 100.0)
            select new DashboardCategoryDto
            {
                Category = cat,
                Language = targetLanguage,
                DisplayName = cat.GetDisplayName(targetLanguage),
                ShortDescription = cat.GetShortDescription(targetLanguage),
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

        var languageProgresses = user.Progresses.Where(p => p.Language == targetLanguage).ToList();
        var totalCompleted = languageProgresses.Sum(p => p.CompletedLevelsCount);
        var overallMastery = (double)totalCompleted / (allCategoryTypes.Length * 5) * 100.0;

        return new UserDashboardDto
        {
            UserId = user.Id,
            Username = user.Username,
            SelectedLanguage = targetLanguage,
            TargetRole = user.TargetRole,
            TotalCompletedExercises = totalCompleted,
            OverallMasteryPercentage = Math.Round(overallMastery, 1),
            Categories = categories
        };
    }
}
