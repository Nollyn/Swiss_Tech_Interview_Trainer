using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

/// <summary>
/// Data transfer object representing the active exercise details displayed to the candidate.
/// </summary>
public sealed record ExerciseDto
{
    /// <summary>
    /// Gets the unique identifier for the exercise.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the interview category enum.
    /// </summary>
    public CategoryType Category { get; init; }

    /// <summary>
    /// Gets the backend programming language for this exercise.
    /// </summary>
    public ProgrammingLanguage Language { get; init; } = ProgrammingLanguage.CSharp;

    /// <summary>
    /// Gets the human-readable display name for the programming language.
    /// </summary>
    public string LanguageDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the human-readable display title for the category.
    /// </summary>
    public string CategoryDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the difficulty tier (Level 1 to 5).
    /// </summary>
    public DifficultyLevel Level { get; init; }

    /// <summary>
    /// Gets the descriptive difficulty label.
    /// </summary>
    public string LevelLabel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the exercise title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the problem statement and scenario description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the starter C# code boilerplate.
    /// </summary>
    public string StarterCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected deliverable format description.
    /// </summary>
    public string ExpectedOutputFormat { get; init; } = string.Empty;

    /// <summary>
    /// Gets the guidance hints available when Hint Mode is active.
    /// </summary>
    public string Hints { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether Hint Mode is currently unlocked for the user.
    /// </summary>
    public bool HintModeActive { get; init; }

    /// <summary>
    /// Gets a value indicating whether this exercise was generated dynamically by an LLM.
    /// </summary>
    public bool IsAiGenerated { get; init; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
