using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

public class Exercise
{
    public Guid Id { get; private set; }
    public CategoryType Category { get; private set; }
    public DifficultyLevel Level { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string StarterCode { get; private set; } = string.Empty;
    public string ExpectedOutputFormat { get; private set; } = string.Empty;
    public string Hints { get; private set; } = string.Empty;
    public Guid? PreviousExerciseReferenceId { get; private set; }
    public bool IsAIGenerated { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<Submission> _submissions = new();
    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();

    private Exercise() { }

    public Exercise(
        CategoryType category,
        DifficultyLevel level,
        string title,
        string description,
        string starterCode,
        string expectedOutputFormat = "C# Source File (.cs) or Architectural Justification",
        string hints = "",
        bool isAiGenerated = false,
        Guid? previousExerciseReferenceId = null)
    {
        Id = Guid.NewGuid();
        Category = category;
        Level = level;
        Title = string.IsNullOrWhiteSpace(title) ? $"{category} - {level}" : title;
        Description = description ?? string.Empty;
        StarterCode = starterCode ?? string.Empty;
        ExpectedOutputFormat = expectedOutputFormat;
        Hints = hints ?? string.Empty;
        IsAIGenerated = isAiGenerated;
        PreviousExerciseReferenceId = previousExerciseReferenceId;
        CreatedAt = DateTime.UtcNow;
    }
}
