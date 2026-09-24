using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Common.Models;

public sealed record ExerciseGenerationContext
{
    public CategoryType Category { get; init; }
    public DifficultyLevel Level { get; init; }
    public string? PreviousExerciseTitle { get; init; }
    public string? PreviousExerciseDescription { get; init; }
    public bool IncludeHintModeContext { get; init; }
}

public sealed record GeneratedExerciseDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StarterCode { get; init; } = string.Empty;
    public string ExpectedOutputFormat { get; init; } = "C# Source Code (.cs)";
    public string Hints { get; init; } = string.Empty;
}

public sealed record EvaluationPromptContext
{
    public CategoryType Category { get; init; }
    public DifficultyLevel Level { get; init; }
    public string ExerciseTitle { get; init; } = string.Empty;
    public string ExerciseDescription { get; init; } = string.Empty;
    public string StarterCode { get; init; } = string.Empty;
    public string SubmittedCode { get; init; } = string.Empty;
    public string AdditionalNotes { get; init; } = string.Empty;
}

public sealed record LlmEvaluationCriterionDto
{
    public string Name { get; init; } = string.Empty;
    public double Weight { get; init; }
    public double Score { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string Rationale { get; init; } = string.Empty;
}

public sealed record LlmCodeSuggestionDto
{
    public string Title { get; init; } = string.Empty;
    public string OriginalCode { get; init; } = string.Empty;
    public string SuggestedCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
}

public sealed record LlmEvaluationResponseDto
{
    public List<LlmEvaluationCriterionDto> Criteria { get; init; } = new();
    public List<LlmCodeSuggestionDto> Suggestions { get; init; } = new();
    public string GeneralFeedback { get; init; } = string.Empty;
    public string ModelUsed { get; init; } = string.Empty;
}
