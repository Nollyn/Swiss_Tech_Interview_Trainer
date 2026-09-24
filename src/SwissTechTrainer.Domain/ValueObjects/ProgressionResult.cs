using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of evaluating a candidate's attempt against domain progression specifications.
/// </summary>
public sealed record ProgressionResult
{
    /// <summary>
    /// Gets a value indicating whether the candidate successfully advanced to the next difficulty level.
    /// </summary>
    public bool LevelAdvanced { get; init; }

    /// <summary>
    /// Gets the difficulty level prior to the evaluated attempt.
    /// </summary>
    public DifficultyLevel OldLevel { get; init; }

    /// <summary>
    /// Gets the newly assigned difficulty level after the evaluated attempt.
    /// </summary>
    public DifficultyLevel NewLevel { get; init; }

    /// <summary>
    /// Gets the current count of consecutive failures on this level.
    /// </summary>
    public int ConsecutiveFailures { get; init; }

    /// <summary>
    /// Gets a value indicating whether Hint Mode (anti-frustration mechanism) was unlocked as a result of this attempt.
    /// </summary>
    public bool HintModeUnlocked { get; init; }

    /// <summary>
    /// Gets the deterministic mathematical score computed for the evaluated attempt (0.0 to 100.0).
    /// </summary>
    public double FinalScore { get; init; }

    /// <summary>
    /// Gets a value indicating whether the final score met or exceeded the 90.0% progression threshold.
    /// </summary>
    public bool ThresholdPassed { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionResult"/> record. Parameterless constructor for serialization.
    /// </summary>
    public ProgressionResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionResult"/> record with domain outcome values.
    /// </summary>
    /// <param name="levelAdvanced">Whether the user advanced to the next level.</param>
    /// <param name="oldLevel">The previous difficulty level.</param>
    /// <param name="newLevel">The updated difficulty level.</param>
    /// <param name="consecutiveFailures">The count of consecutive failed attempts.</param>
    /// <param name="hintModeUnlocked">Whether hint mode is now active.</param>
    /// <param name="finalScore">The aggregated final score.</param>
    /// <param name="thresholdPassed">Whether the threshold (>= 90%) was passed.</param>
    public ProgressionResult(
        bool levelAdvanced,
        DifficultyLevel oldLevel,
        DifficultyLevel newLevel,
        int consecutiveFailures,
        bool hintModeUnlocked,
        double finalScore,
        bool thresholdPassed)
    {
        LevelAdvanced = levelAdvanced;
        OldLevel = oldLevel;
        NewLevel = newLevel;
        ConsecutiveFailures = consecutiveFailures;
        HintModeUnlocked = hintModeUnlocked;
        FinalScore = finalScore;
        ThresholdPassed = thresholdPassed;
    }
}
