using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Submissions;

/// <summary>
/// Data transfer object representing the evaluation outcome of an individual rubric criterion.
/// </summary>
public sealed record CriterionEvaluationResultDto
{
    /// <summary>
    /// Gets the rubric dimension name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the relative percentage weight.
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Gets the assigned score (0.0 to 100.0).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the cited code evidence from the submission.
    /// </summary>
    public string Evidence { get; init; } = string.Empty;

    /// <summary>
    /// Gets the constructive reasoning behind the score.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;
}

/// <summary>
/// Data transfer object representing a before/after code suggestion.
/// </summary>
public sealed record CodeSuggestionResultDto
{
    /// <summary>
    /// Gets the suggestion title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the candidate's original code snippet.
    /// </summary>
    public string OriginalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the suggested refactored snippet.
    /// </summary>
    public string SuggestedCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the explanation of why the change is recommended.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;
}

/// <summary>
/// Data transfer object returned upon completing submission evaluation, aggregating scores, breakdown, feedback, and progression updates.
/// </summary>
public sealed record EvaluationResultDto
{
    /// <summary>
    /// Gets the submission identifier.
    /// </summary>
    public Guid SubmissionId { get; init; }

    /// <summary>
    /// Gets the exercise identifier.
    /// </summary>
    public Guid ExerciseId { get; init; }

    /// <summary>
    /// Gets the category type.
    /// </summary>
    public CategoryType Category { get; init; }

    /// <summary>
    /// Gets the formatted category display name.
    /// </summary>
    public string CategoryDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the difficulty tier.
    /// </summary>
    public DifficultyLevel Level { get; init; }

    /// <summary>
    /// Gets the difficulty tier label.
    /// </summary>
    public string LevelLabel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the final aggregated deterministic score (0.0 to 100.0).
    /// </summary>
    public double DeterministicScore { get; init; }

    /// <summary>
    /// Gets a value indicating whether the candidate passed the 90% threshold.
    /// </summary>
    public bool PassedThreshold { get; init; }

    /// <summary>
    /// Gets the markdown feedback synthesis.
    /// </summary>
    public string GeneralFeedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed criteria breakdown.
    /// </summary>
    public List<CriterionEvaluationResultDto> CriteriaBreakdown { get; init; } = [];

    /// <summary>
    /// Gets the actionable before/after code suggestions.
    /// </summary>
    public List<CodeSuggestionResultDto> CodeSuggestions { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the user advanced to the next level.
    /// </summary>
    public bool LevelAdvanced { get; init; }

    /// <summary>
    /// Gets the new difficulty level assigned.
    /// </summary>
    public DifficultyLevel NewLevel { get; init; }

    /// <summary>
    /// Gets the current count of consecutive failures.
    /// </summary>
    public int ConsecutiveFailures { get; init; }

    /// <summary>
    /// Gets a value indicating whether Hint Mode is unlocked.
    /// </summary>
    public bool HintModeUnlocked { get; init; }

    /// <summary>
    /// Gets the evaluation completion UTC timestamp.
    /// </summary>
    public DateTime EvaluatedAt { get; init; }

    /// <summary>
    /// Gets the name of the LLM model used.
    /// </summary>
    public string ModelUsed { get; init; } = string.Empty;

    /// <summary>
    /// Gets the execution duration in milliseconds.
    /// </summary>
    public long ProcessingDurationMs { get; init; }
}

/// <summary>
/// Data transfer object representing a historical candidate attempt item for history views.
/// </summary>
public sealed record CategoryAttemptHistoryItemDto
{
    /// <summary>
    /// Gets the submission identifier.
    /// </summary>
    public Guid SubmissionId { get; init; }

    /// <summary>
    /// Gets the exercise identifier.
    /// </summary>
    public Guid ExerciseId { get; init; }

    /// <summary>
    /// Gets the exercise title.
    /// </summary>
    public string ExerciseTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets the difficulty level attempted.
    /// </summary>
    public DifficultyLevel Level { get; init; }

    /// <summary>
    /// Gets the deterministic score achieved.
    /// </summary>
    public double DeterministicScore { get; init; }

    /// <summary>
    /// Gets a value indicating whether this attempt passed the threshold.
    /// </summary>
    public bool PassedThreshold { get; init; }

    /// <summary>
    /// Gets the submission timestamp.
    /// </summary>
    public DateTime SubmittedAt { get; init; }

    /// <summary>
    /// Gets the evaluation feedback.
    /// </summary>
    public string GeneralFeedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets a truncated preview of the submitted source code.
    /// </summary>
    public string SubmittedCodePreview { get; init; } = string.Empty;
}
