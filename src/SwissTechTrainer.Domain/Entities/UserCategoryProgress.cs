using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

/// <summary>
/// Represents a user's progression state in a specific interview category, tracking current level, attempts, scores, and anti-frustration status.
/// </summary>
public class UserCategoryProgress
{
    /// <summary>
    /// Gets the unique identifier for this category progress record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the interview category.
    /// </summary>
    public CategoryType Category { get; private set; }

    /// <summary>
    /// Gets the current difficulty level reached (Level 1 to 5).
    /// </summary>
    public DifficultyLevel CurrentLevel { get; private set; } = DifficultyLevel.Level1;

    /// <summary>
    /// Gets the number of consecutive failed attempts on the current level.
    /// </summary>
    public int ConsecutiveFailures { get; private set; }

    /// <summary>
    /// Gets the total count of completed levels in this category.
    /// </summary>
    public int CompletedLevelsCount { get; private set; }

    /// <summary>
    /// Gets the highest score ever achieved in this category (0.0 to 100.0).
    /// </summary>
    public double HighestScoreAchieved { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Hint Mode (anti-frustration mechanism triggered after 3+ consecutive failures) is active.
    /// </summary>
    public bool HintModeActive { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp of the most recent evaluation attempt, or null if none.
    /// </summary>
    public DateTime? LastAttemptAt { get; private set; }

    /// <summary>
    /// Gets the navigation property to the user profile.
    /// </summary>
    public UserProfile? User { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core.
    /// </summary>
    private UserCategoryProgress() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCategoryProgress"/> entity.
    /// </summary>
    /// <param name="userId">The ID of the candidate.</param>
    /// <param name="category">The interview category.</param>
    /// <exception cref="ArgumentException">Thrown when userId is empty.</exception>
    public UserCategoryProgress(Guid userId, CategoryType category)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
        CurrentLevel = DifficultyLevel.Level1;
        ConsecutiveFailures = 0;
        CompletedLevelsCount = 0;
        HighestScoreAchieved = 0.0;
        HintModeActive = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new <see cref="UserCategoryProgress"/> record at Level 1.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new <see cref="UserCategoryProgress"/> instance.</returns>
    public static UserCategoryProgress Create(Guid userId, CategoryType category)
    {
        return new UserCategoryProgress(userId, category);
    }

    /// <summary>
    /// Records the outcome of an evaluation attempt, advancing difficulty if passed or incrementing failure counters.
    /// </summary>
    /// <param name="finalScore">The score achieved on the attempt.</param>
    /// <param name="passed">Whether the score met the progression threshold.</param>
    public void RecordAttempt(double finalScore, bool passed)
    {
        LastAttemptAt = DateTime.UtcNow;
        if (finalScore > HighestScoreAchieved)
        {
            HighestScoreAchieved = finalScore;
        }

        if (passed)
        {
            ConsecutiveFailures = 0;
            HintModeActive = false;
            CompletedLevelsCount++;

            // Advance level if not already at maximum (Level 5)
            if (CurrentLevel < DifficultyLevel.Level5)
            {
                CurrentLevel = (DifficultyLevel)((int)CurrentLevel + 1);
            }
        }
        else
        {
            ConsecutiveFailures++;
            if (ConsecutiveFailures >= 3)
            {
                HintModeActive = true;
            }
        }
    }

    /// <summary>
    /// Manually resets the consecutive failures counter and deactivates hint mode.
    /// </summary>
    public void ResetFailures()
    {
        ConsecutiveFailures = 0;
        HintModeActive = false;
    }
}
