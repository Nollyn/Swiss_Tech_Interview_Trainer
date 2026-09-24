using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

public class UserCategoryProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public CategoryType Category { get; private set; }
    public DifficultyLevel CurrentLevel { get; private set; } = DifficultyLevel.Level1;
    public int ConsecutiveFailures { get; private set; }
    public int CompletedLevelsCount { get; private set; }
    public double HighestScoreAchieved { get; private set; }
    public bool HintModeActive { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; private set; }

    // Navigation
    public UserProfile? User { get; private set; }

    // EF constructor
    private UserCategoryProgress() { }

    public UserCategoryProgress(Guid userId, CategoryType category)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
        CurrentLevel = DifficultyLevel.Level1;
        ConsecutiveFailures = 0;
        CompletedLevelsCount = 0;
        HighestScoreAchieved = 0;
        HintModeActive = false;
        CreatedAt = DateTime.UtcNow;
    }

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

    public void ResetFailures()
    {
        ConsecutiveFailures = 0;
        HintModeActive = false;
    }
}
