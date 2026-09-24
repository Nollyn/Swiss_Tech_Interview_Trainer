namespace SwissTechTrainer.Domain.Enums;

public enum CategoryType
{
    Coding = 1,
    SystemDesign = 2,
    DotNetDeepDive = 3,
    CleanCode = 4,
    ApiDesign = 5,
    Testing = 6,
    Behavioral = 7
}

public static class CategoryTypeExtensions
{
    public static string GetDisplayName(this CategoryType category) => category switch
    {
        CategoryType.Coding => "Coding / Algorithms & Data Structures",
        CategoryType.SystemDesign => "System Design",
        CategoryType.DotNetDeepDive => ".NET / C# Deep Dive",
        CategoryType.CleanCode => "Clean Code & Refactoring",
        CategoryType.ApiDesign => "API Design & Distributed Systems",
        CategoryType.Testing => "Testing & TDD",
        CategoryType.Behavioral => "Behavioral / Leadership (STAR)",
        _ => category.ToString()
    };

    public static string GetShortDescription(this CategoryType category) => category switch
    {
        CategoryType.Coding => "Pragmatic algorithms, time/space complexity, and idiomatic C# structures.",
        CategoryType.SystemDesign => "Distributed service design, resilience, and trade-off justifications.",
        CategoryType.DotNetDeepDive => "Memory management, async/await internals, GC, and CLR performance.",
        CategoryType.CleanCode => "Refactoring code smells, SOLID compliance, and technical debt reduction.",
        CategoryType.ApiDesign => "REST/gRPC contracts, idempotency, retry policies, and circuit breakers.",
        CategoryType.Testing => "Unit, integration, and mutation testing using strict TDD principles.",
        CategoryType.Behavioral => "Technical leadership, conflict resolution, and architectural ownership using STAR.",
        _ => string.Empty
    };

    public static string GetIcon(this CategoryType category) => category switch
    {
        CategoryType.Coding => "bi-code-slash",
        CategoryType.SystemDesign => "bi-diagram-3",
        CategoryType.DotNetDeepDive => "bi-cpu",
        CategoryType.CleanCode => "bi-brush",
        CategoryType.ApiDesign => "bi-hdd-network",
        CategoryType.Testing => "bi-check2-circle",
        CategoryType.Behavioral => "bi-people",
        _ => "bi-journal-code"
    };
}
