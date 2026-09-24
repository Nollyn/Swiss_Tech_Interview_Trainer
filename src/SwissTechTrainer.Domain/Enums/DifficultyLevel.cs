namespace SwissTechTrainer.Domain.Enums;

public enum DifficultyLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5
}

public static class DifficultyLevelExtensions
{
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
