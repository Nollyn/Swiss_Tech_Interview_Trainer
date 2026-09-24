using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Specifications;

/// <summary>
/// Encapsulates the domain specification for level unlock gating, progression evaluation, and anti-frustration triggering.
/// </summary>
public static class ProgressionSpecification
{
    /// <summary>
    /// The mathematical threshold (90.0%) required to unlock the subsequent difficulty tier.
    /// </summary>
    public const double UnlockThreshold = 90.0;

    /// <summary>
    /// The consecutive failure count threshold (3) required to unlock Hint Mode.
    /// </summary>
    public const int AntiFrustrationThreshold = 3;

    /// <summary>
    /// Evaluates candidate progression against domain rules given the current state and evaluated attempt score.
    /// </summary>
    /// <param name="progress">The current progression aggregate for the category.</param>
    /// <param name="finalScore">The aggregated deterministic score achieved on the submission.</param>
    /// <returns>A <see cref="ProgressionResult"/> detailing whether level advancement or hint activation occurred.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="progress"/> is null.</exception>
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
