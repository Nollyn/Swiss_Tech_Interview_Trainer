namespace SwissTechTrainer.Domain.Enums;

/// <summary>
/// Represents the adaptive 5-level difficulty progression ladder for technical exercises.
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// Level 1: Core Fundamentals and language/framework idioms.
    /// </summary>
    Level1 = 1,

    /// <summary>
    /// Level 2: Applied Production Problems with real-world requirements.
    /// </summary>
    Level2 = 2,

    /// <summary>
    /// Level 3: Concurrency, high performance, and edge-case pressure.
    /// </summary>
    Level3 = 3,

    /// <summary>
    /// Level 4: Distributed Systems, failure modes, and architectural refactoring.
    /// </summary>
    Level4 = 4,

    /// <summary>
    /// Level 5: Senior/Tech Lead High-Constraint Mastery and strategic trade-offs.
    /// </summary>
    Level5 = 5
}

/// <summary>
/// Extension methods for formatting and describing <see cref="DifficultyLevel"/>.
/// </summary>
public static class DifficultyLevelExtensions
{
    /// <summary>
    /// Gets the human-friendly label and description for the difficulty level.
    /// </summary>
    /// <param name="level">The difficulty level to format.</param>
    /// <returns>A formatted level label suitable for UI display.</returns>
    public static string GetLabel(this DifficultyLevel level) => level switch
    {
        DifficultyLevel.Level1 => "Level 1: Core Fundamentals",
        DifficultyLevel.Level2 => "Level 2: Applied Production Problems",
        DifficultyLevel.Level3 => "Level 3: Concurrency & Edge Pressure",
        DifficultyLevel.Level4 => "Level 4: Distributed Systems & Refactoring",
        DifficultyLevel.Level5 => "Level 5: Senior/Tech Lead High-Constraint Mastery",
        _ => level.ToString()
    };
}
