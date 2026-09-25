using FluentAssertions;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Specifications;
using Xunit;

namespace SwissTechTrainer.Domain.Tests;

public class ProgressionSpecificationTests
{
    [Fact]
    public void EvaluateProgression_WhenScoreIsAbove90_AdvancesLevelAndResetsFailures()
    {
        var progress = new UserCategoryProgress(Guid.NewGuid(), CategoryType.Coding);
        // Simulate previous failure
        progress.RecordAttempt(70.0, false);
        progress.ConsecutiveFailures.Should().Be(1);

        var result = ProgressionSpecification.EvaluateProgression(progress, 94.5);

        result.ThresholdPassed.Should().BeTrue();
        result.LevelAdvanced.Should().BeTrue();
        result.OldLevel.Should().Be(DifficultyLevel.Level1);
        result.NewLevel.Should().Be(DifficultyLevel.Level2);
        result.ConsecutiveFailures.Should().Be(0);
        result.HintModeUnlocked.Should().BeFalse();
    }

    [Fact]
    public void EvaluateProgression_WhenAlreadyAtMaxLevel5_DoesNotExceedMaxLevel()
    {
        var progress = new UserCategoryProgress(Guid.NewGuid(), CategoryType.CleanCode);
        // Advance to Level 5
        progress.RecordAttempt(95.0, true);
        progress.RecordAttempt(95.0, true);
        progress.RecordAttempt(95.0, true);
        progress.RecordAttempt(95.0, true);
        progress.CurrentLevel.Should().Be(DifficultyLevel.Level5);

        var result = ProgressionSpecification.EvaluateProgression(progress, 98.0);

        result.ThresholdPassed.Should().BeTrue();
        result.LevelAdvanced.Should().BeFalse();
        result.OldLevel.Should().Be(DifficultyLevel.Level5);
        result.NewLevel.Should().Be(DifficultyLevel.Level5);
    }

    [Fact]
    public void EvaluateProgression_WhenFailingThreeTimes_UnlocksHintMode()
    {
        var progress = new UserCategoryProgress(Guid.NewGuid(), CategoryType.SystemDesign);
        
        // 1st failure
        var res1 = ProgressionSpecification.EvaluateProgression(progress, 75.0);
        progress.RecordAttempt(75.0, false);
        res1.ConsecutiveFailures.Should().Be(1);
        res1.HintModeUnlocked.Should().BeFalse();

        // 2nd failure
        var res2 = ProgressionSpecification.EvaluateProgression(progress, 82.0);
        progress.RecordAttempt(82.0, false);
        res2.ConsecutiveFailures.Should().Be(2);
        res2.HintModeUnlocked.Should().BeFalse();

        // 3rd failure -> triggers anti-frustration
        var res3 = ProgressionSpecification.EvaluateProgression(progress, 88.0);
        progress.RecordAttempt(88.0, false);
        res3.ConsecutiveFailures.Should().Be(3);
        res3.HintModeUnlocked.Should().BeTrue();
        progress.HintModeActive.Should().BeTrue();
    }

    [Fact]
    public void UserCategoryProgress_WhenPassingAfterHintMode_DeactivatesHintMode()
    {
        var progress = new UserCategoryProgress(Guid.NewGuid(), CategoryType.LanguageDeepDive);
        progress.RecordAttempt(70.0, false);
        progress.RecordAttempt(70.0, false);
        progress.RecordAttempt(70.0, false);
        progress.HintModeActive.Should().BeTrue();

        // Now passes
        progress.RecordAttempt(92.0, true);

        progress.HintModeActive.Should().BeFalse();
        progress.ConsecutiveFailures.Should().Be(0);
        progress.CurrentLevel.Should().Be(DifficultyLevel.Level2);
    }
}
