using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.ValueObjects;

public sealed record ProgressionResult
{
    public bool LevelAdvanced { get; init; }
    public DifficultyLevel OldLevel { get; init; }
    public DifficultyLevel NewLevel { get; init; }
    public int ConsecutiveFailures { get; init; }
    public bool HintModeUnlocked { get; init; }
    public double FinalScore { get; init; }
    public bool ThresholdPassed { get; init; }

    public ProgressionResult() { }

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
