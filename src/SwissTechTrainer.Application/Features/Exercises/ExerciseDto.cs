using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

public sealed record ExerciseDto
{
    public Guid Id { get; init; }
    public CategoryType Category { get; init; }
    public string CategoryDisplayName { get; init; } = string.Empty;
    public DifficultyLevel Level { get; init; }
    public string LevelLabel { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StarterCode { get; init; } = string.Empty;
    public string ExpectedOutputFormat { get; init; } = string.Empty;
    public string Hints { get; init; } = string.Empty;
    public bool HintModeActive { get; init; }
    public bool IsAiGenerated { get; init; }
    public DateTime CreatedAt { get; init; }
}
