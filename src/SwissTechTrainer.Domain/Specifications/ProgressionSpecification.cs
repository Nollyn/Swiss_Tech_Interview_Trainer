using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Specifications;

public static class ProgressionSpecification
{
    public const double UnlockThreshold = 90.0;
    public const int AntiFrustrationThreshold = 3;

    public static ProgressionResult EvaluateProgression(UserCategoryProgress progress, double finalScore)
    {
        ArgumentNullException.ThrowIfNull(progress);

        bool thresholdPassed = finalScore >= UnlockThreshold;
        DifficultyLevel oldLevel = progress.CurrentLevel;
        DifficultyLevel newLevel = oldLevel;
        int newConsecutiveFailures;
        bool hintModeUnlocked;

        if (thresholdPassed)
        {
            newConsecutiveFailures = 0;
            hintModeUnlocked = false;

            if (oldLevel < DifficultyLevel.Level5)
            {
                newLevel = (DifficultyLevel)((int)oldLevel + 1);
            }
        }
        else
        {
            newConsecutiveFailures = progress.ConsecutiveFailures + 1;
            hintModeUnlocked = newConsecutiveFailures >= AntiFrustrationThreshold;
        }

        bool levelAdvanced = newLevel > oldLevel;

        return new ProgressionResult(
            levelAdvanced: levelAdvanced,
            oldLevel: oldLevel,
            newLevel: newLevel,
            consecutiveFailures: newConsecutiveFailures,
            hintModeUnlocked: hintModeUnlocked,
            finalScore: finalScore,
            thresholdPassed: thresholdPassed
        );
    }
}
