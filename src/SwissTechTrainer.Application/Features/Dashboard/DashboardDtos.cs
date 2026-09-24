using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Application.Features.Dashboard;

public sealed record DashboardCategoryDto
{
    public CategoryType Category { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public DifficultyLevel CurrentLevel { get; init; }
    public string CurrentLevelLabel { get; init; } = string.Empty;
    public int CompletedLevelsCount { get; init; }
    public double ProgressPercentage { get; init; }
    public double HighestScoreAchieved { get; init; }
    public int ConsecutiveFailures { get; init; }
    public bool HintModeActive { get; init; }
    public DateTime? LastAttemptAt { get; init; }
}

public sealed record UserDashboardDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string TargetRole { get; init; } = string.Empty;
    public int TotalCompletedExercises { get; init; }
    public double OverallMasteryPercentage { get; init; }
    public List<DashboardCategoryDto> Categories { get; init; } = new();
}
