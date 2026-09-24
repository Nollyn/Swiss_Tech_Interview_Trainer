using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Common.Models;

/// <summary>
/// Context parameters passed to the LLM exercise generation pipeline.
/// </summary>
public sealed record ExerciseGenerationContext
{
    /// <summary>
    /// Gets the interview category to generate an exercise for.
    /// </summary>
    public CategoryType Category { get; init; }

    /// <summary>
    /// Gets the target difficulty level.
    /// </summary>
    public DifficultyLevel Level { get; init; }

    /// <summary>
    /// Gets the optional title of the candidate's previous exercise in this category for negative prompting.
    /// </summary>
    public string? PreviousExerciseTitle { get; init; }

    /// <summary>
    /// Gets the optional description of the previous exercise for negative prompting.
    /// </summary>
    public string? PreviousExerciseDescription { get; init; }

    /// <summary>
    /// Gets a value indicating whether Hint Mode is unlocked and extra hints should be provided.
    /// </summary>
    public bool IncludeHintModeContext { get; init; }
}

/// <summary>
/// Structured DTO returned from the LLM exercise generation prompt.
/// </summary>
public sealed record GeneratedExerciseDto
{
    /// <summary>
    /// Gets the exercise title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed problem statement and requirements.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the starter boilerplate code.
    /// </summary>
    public string StarterCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected deliverable format description.
    /// </summary>
    public string ExpectedOutputFormat { get; init; } = "C# Source Code (.cs)";

    /// <summary>
    /// Gets architectural hints to assist struggling candidates.
    /// </summary>
    public string Hints { get; init; } = string.Empty;
}

/// <summary>
/// Context payload provided to the LLM evaluator prompt.
/// </summary>
public sealed record EvaluationPromptContext
{
    /// <summary>
    /// Gets the interview category.
    /// </summary>
    public CategoryType Category { get; init; }

    /// <summary>
    /// Gets the difficulty level.
    /// </summary>
    public DifficultyLevel Level { get; init; }

    /// <summary>
    /// Gets the exercise title.
    /// </summary>
    public string ExerciseTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets the exercise description and requirements.
    /// </summary>
    public string ExerciseDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original starter code.
    /// </summary>
    public string StarterCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the candidate's submitted code or justification.
    /// </summary>
    public string SubmittedCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional candidate notes or explanations.
    /// </summary>
    public string AdditionalNotes { get; init; } = string.Empty;
}

/// <summary>
/// Represents an individual criterion evaluation item returned by the LLM.
/// </summary>
public sealed record LlmEvaluationCriterionDto
{
    /// <summary>
    /// Gets the criterion name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the relative weight.
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Gets the raw score awarded by the LLM (0.0 to 100.0).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the cited textual code evidence.
    /// </summary>
    public string Evidence { get; init; } = string.Empty;

    /// <summary>
    /// Gets the constructive reasoning.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;
}

/// <summary>
/// Represents a refactoring code diff suggestion returned by the LLM.
/// </summary>
public sealed record LlmCodeSuggestionDto
{
    /// <summary>
    /// Gets the suggestion title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the candidate's original code.
    /// </summary>
    public string OriginalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the suggested refactored code.
    /// </summary>
    public string SuggestedCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the explanation of architectural benefit.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;
}

/// <summary>
/// Structured DTO representing the complete response parsed from the LLM evaluator call.
/// </summary>
public sealed record LlmEvaluationResponseDto
{
    /// <summary>
    /// Gets the evaluated criteria breakdown.
    /// </summary>
    public List<LlmEvaluationCriterionDto> Criteria { get; init; } = [];

    /// <summary>
    /// Gets the actionable code improvement suggestions.
    /// </summary>
    public List<LlmCodeSuggestionDto> Suggestions { get; init; } = [];

    /// <summary>
    /// Gets the narrative general feedback in Markdown format.
    /// </summary>
    public string GeneralFeedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets the model name or identifier that produced the assessment.
    /// </summary>
    public string ModelUsed { get; init; } = string.Empty;
}
