using FluentAssertions;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.ValueObjects;
using Xunit;

namespace SwissTechTrainer.Domain.Tests;

public class DeterministicScoringTests
{
    [Fact]
    public void CalculateWeightedScore_WithStandardWeights_CalculatesExactWeightedAverage()
    {
        // Clean Code Rubric: SOLID (30%), Readability (20%), Edge Cases (25%), Performance (25%)
        var criteria = new List<CriterionScore>
        {
            new("SOLID Principles Compliance", 30.0, 95.0, "Refactored to DIP", "Excellent separation of interfaces"),
            new("Readability & Clean Naming", 20.0, 90.0, "Descriptive variables", "Clear method names"),
            new("Edge Case & Defensive Handling", 25.0, 80.0, "Missing null check on line 14", "Mostly solid"),
            new("Performance & Allocations", 25.0, 100.0, "Zero-alloc span parsing", "Optimal execution")
        };

        // Expected: (95*30 + 90*20 + 80*25 + 100*25) / 100 = (2850 + 1800 + 2000 + 2500) / 100 = 9150 / 100 = 91.5
        double score = Evaluation.CalculateWeightedScore(criteria);

        score.Should().Be(91.5);
    }

    [Fact]
    public void CalculateWeightedScore_WhenEmpty_ReturnsZero()
    {
        double score = Evaluation.CalculateWeightedScore(new List<CriterionScore>());
        score.Should().Be(0.0);
    }

    [Fact]
    public void CalculateWeightedScore_WhenWeightsDoNotSumToOneHundred_NormalizesCorrectly()
    {
        var criteria = new List<CriterionScore>
        {
            new("C1", 2.0, 80.0, "Evidence", "Rationale"),
            new("C2", 2.0, 100.0, "Evidence", "Rationale")
        };

        // (80*2 + 100*2) / 4 = 360 / 4 = 90.0
        double score = Evaluation.CalculateWeightedScore(criteria);
        score.Should().Be(90.0);
    }

    [Fact]
    public void CompleteEvaluation_SetsPassedThresholdTrue_WhenScoreIsAtOrAbove90()
    {
        var evaluation = new Evaluation(Guid.NewGuid());
        var criteria = new List<CriterionScore>
        {
            new("C1", 50.0, 90.0, "Evidence", "Rationale"),
            new("C2", 50.0, 90.0, "Evidence", "Rationale")
        };

        evaluation.Complete(criteria, null!, "Great work", "llama-3.3-70b-versatile", 450);

        evaluation.DeterministicScore.Should().Be(90.0);
        evaluation.PassedThreshold.Should().BeTrue();
        evaluation.Status.Should().Be(Enums.EvaluationStatus.Completed);
    }

    [Fact]
    public void CompleteEvaluation_SetsPassedThresholdFalse_WhenScoreIsBelow90()
    {
        var evaluation = new Evaluation(Guid.NewGuid());
        var criteria = new List<CriterionScore>
        {
            new("C1", 50.0, 89.9, "Evidence", "Rationale"),
            new("C2", 50.0, 89.9, "Evidence", "Rationale")
        };

        evaluation.Complete(criteria, null!, "Needs refactoring", "llama-3.3-70b-versatile", 450);

        evaluation.DeterministicScore.Should().Be(89.9);
        evaluation.PassedThreshold.Should().BeFalse();
        evaluation.Status.Should().Be(Enums.EvaluationStatus.Completed);
    }
}
