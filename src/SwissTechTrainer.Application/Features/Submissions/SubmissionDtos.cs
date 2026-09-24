using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Submissions;

public sealed record CriterionEvaluationResultDto
{
    public string Name { get; init; } = string.Empty;
    public double Weight { get; init; }
    public double Score { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string Rationale { get; init; } = string.Empty;
}

public sealed record CodeSuggestionResultDto
{
    public string Title { get; init; } = string.Empty;
    public string OriginalCode { get; init; } = string.Empty;
    public string SuggestedCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
}

public sealed record EvaluationResultDto
{
    public Guid SubmissionId { get; init; }
    public Guid ExerciseId { get; init; }
    public CategoryType Category { get; init; }
    public string CategoryDisplayName { get; init; } = string.Empty;
    public DifficultyLevel Level { get; init; }
    public string LevelLabel { get; init; } = string.Empty;
    public double DeterministicScore { get; init; }
    public bool PassedThreshold { get; init; }
    public string GeneralFeedback { get; init; } = string.Empty;
    public List<CriterionEvaluationResultDto> CriteriaBreakdown { get; init; } = new();
    public List<CodeSuggestionResultDto> CodeSuggestions { get; init; } = new();
    public bool LevelAdvanced { get; init; }
    public DifficultyLevel NewLevel { get; init; }
    public int ConsecutiveFailures { get; init; }
    public bool HintModeUnlocked { get; init; }
    public DateTime EvaluatedAt { get; init; }
    public string ModelUsed { get; init; } = string.Empty;
    public long ProcessingDurationMs { get; init; }
}

public sealed record CategoryAttemptHistoryItemDto
{
    public Guid SubmissionId { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseTitle { get; init; } = string.Empty;
    public DifficultyLevel Level { get; init; }
    public double DeterministicScore { get; init; }
    public bool PassedThreshold { get; init; }
    public DateTime SubmittedAt { get; init; }
    public string GeneralFeedback { get; init; } = string.Empty;
    public string SubmittedCodePreview { get; init; } = string.Empty;
}
