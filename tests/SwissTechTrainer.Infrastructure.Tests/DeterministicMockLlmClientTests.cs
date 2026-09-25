using FluentAssertions;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;
using SwissTechTrainer.Infrastructure.LLM;
using Xunit;

namespace SwissTechTrainer.Infrastructure.Tests;

public class DeterministicMockLlmClientTests
{
    private readonly DeterministicMockLlmClient _sut = new();

    [Fact]
    public async Task GenerateExerciseAsync_ReturnsValidExerciseMatchingCategoryAndLevel()
    {
        var context = new ExerciseGenerationContext
        {
            Category = CategoryType.LanguageDeepDive,
            Level = DifficultyLevel.Level2,
            IncludeHintModeContext = true
        };

        var result = await _sut.GenerateExerciseAsync(context);

        result.Should().NotBeNull();
        result.Title.Should().Contain(".NET / C# Deep Dive");
        result.Title.Should().Contain("Level 2");
        result.Description.Should().NotBeNullOrWhiteSpace();
        result.StarterCode.Should().NotBeNullOrWhiteSpace();
        result.Hints.Should().Contain("Diagnostic Hint");
    }

    [Fact]
    public async Task EvaluateSubmissionAsync_ReturnsAllOfficialRubricCriteria()
    {
        var officialRubrics = CategoryRubricCatalog.GetRubricForCategory(CategoryType.ApiDesign);
        var context = new EvaluationPromptContext
        {
            Category = CategoryType.ApiDesign,
            Level = DifficultyLevel.Level1,
            ExerciseTitle = "Idempotent API",
            SubmittedCode = "public class TransferController : ControllerBase { [HttpPost] public async Task<IActionResult> Post() { throw new ArgumentNullException(); } }"
        };

        var result = await _sut.EvaluateSubmissionAsync(context);

        result.Should().NotBeNull();
        result.Criteria.Should().HaveCount(officialRubrics.Count);
        result.Suggestions.Should().NotBeEmpty();
        result.GeneralFeedback.Should().NotBeNullOrWhiteSpace();
    }
}
