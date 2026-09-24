using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Dashboard;

public sealed record GetUserDashboardQuery(Guid? UserId = null) : IRequest<UserDashboardDto>;

public sealed class GetUserDashboardQueryHandler : IRequestHandler<GetUserDashboardQuery, UserDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserDashboardDto> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await _currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var user = await _context.UserProfiles
            .Include(u => u.Progresses)
            .Include(u => u.Submissions)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user == null)
        {
            user = new UserProfile(_currentUserService.Username, "candidate@swisstech.ch");
            _context.UserProfiles.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var categories = new List<DashboardCategoryDto>();
        var allCategoryTypes = Enum.GetValues<CategoryType>();

        foreach (var cat in allCategoryTypes)
        {
            var progress = user.GetOrCreateProgress(cat);
            int currentLevelInt = (int)progress.CurrentLevel;
            
            // Progress percentage: completed levels out of 5 max
            double progressPercent = Math.Min(100.0, (progress.CompletedLevelsCount / 5.0) * 100.0);

            categories.Add(new DashboardCategoryDto
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
            });
        }

        int totalCompleted = user.Progresses.Sum(p => p.CompletedLevelsCount);
        double overallMastery = (double)totalCompleted / (allCategoryTypes.Length * 5) * 100.0;

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
