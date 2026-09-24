using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Dashboard;

/// <summary>
/// Represents the user's current progress and status in a single interview category dimension on the dashboard.
/// </summary>
public sealed record DashboardCategoryDto
{
    /// <summary>
    /// Gets the interview category dimension.
    /// </summary>
    public CategoryType Category { get; init; }

    /// <summary>
    /// Gets the human-readable display title.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the short description of the assessed skills.
    /// </summary>
    public string ShortDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Bootstrap icon class name.
    /// </summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current difficulty level reached (Level 1 to 5).
    /// </summary>
    public DifficultyLevel CurrentLevel { get; init; }

    /// <summary>
    /// Gets the formatted display label for the current level.
    /// </summary>
    public string CurrentLevelLabel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the count of successfully mastered difficulty levels.
    /// </summary>
    public int CompletedLevelsCount { get; init; }

    /// <summary>
    /// Gets the percentage of category mastery completed (0.0 to 100.0%).
    /// </summary>
    public double ProgressPercentage { get; init; }

    /// <summary>
    /// Gets the highest deterministic score achieved in this category.
    /// </summary>
    public double HighestScoreAchieved { get; init; }

    /// <summary>
    /// Gets the count of consecutive failed attempts.
    /// </summary>
    public int ConsecutiveFailures { get; init; }

    /// <summary>
    /// Gets a value indicating whether Hint Mode is currently active for this category.
    /// </summary>
    public bool HintModeActive { get; init; }

    /// <summary>
    /// Gets the UTC timestamp of the most recent evaluation attempt, or null if none.
    /// </summary>
    public DateTime? LastAttemptAt { get; init; }
}

/// <summary>
/// Represents the comprehensive dashboard view model for a candidate, including overall mastery and category metrics.
/// </summary>
public sealed record UserDashboardDto
{
    /// <summary>
    /// Gets the candidate user ID.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the candidate's username.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target role description.
    /// </summary>
    public string TargetRole { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total number of successfully passed exercises.
    /// </summary>
    public int TotalCompletedExercises { get; init; }

    /// <summary>
    /// Gets the aggregated percentage of mastery across all 7 interview categories.
    /// </summary>
    public double OverallMasteryPercentage { get; init; }

    /// <summary>
    /// Gets the collection of category summaries for the dashboard grid.
    /// </summary>
    public List<DashboardCategoryDto> Categories { get; init; } = [];
}
