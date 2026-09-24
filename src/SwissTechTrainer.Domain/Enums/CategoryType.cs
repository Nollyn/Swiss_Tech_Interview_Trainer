namespace SwissTechTrainer.Domain.Enums;

/// <summary>
/// Defines the technical interview assessment dimensions calibrated for the Swiss tech market (Zurich fintech/banking/enterprise standards).
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// Pragmatic algorithms, data structures, and computational complexity in C#.
    /// </summary>
    Coding = 1,

    /// <summary>
    /// Distributed systems architecture, scalability, resilience, and CAP theorem trade-offs at Tech Lead scope.
    /// </summary>
    SystemDesign = 2,

    /// <summary>
    /// CLR execution internals, memory management (GC, Span, Memory), and asynchronous concurrency.
    /// </summary>
    DotNetDeepDive = 3,

    /// <summary>
    /// Refactoring legacy or smelly code to SOLID compliance and architectural cleanliness.
    /// </summary>
    CleanCode = 4,

    /// <summary>
    /// RESTful/gRPC API contracts, idempotency, rate limiting, and distributed fault tolerance.
    /// </summary>
    ApiDesign = 5,

    /// <summary>
    /// Unit, integration, and mutation testing using strict Test-Driven Development (TDD) principles.
    /// </summary>
    Testing = 6,

    /// <summary>
    /// Behavioral and leadership scenarios evaluated using the STAR (Situation, Task, Action, Result) methodology.
    /// </summary>
    Behavioral = 7
}

/// <summary>
/// Provides presentation and display extensions for <see cref="CategoryType"/>.
/// </summary>
public static class CategoryTypeExtensions
{
    /// <summary>
    /// Gets the human-readable display title for the specified interview category.
    /// </summary>
    /// <param name="category">The category type to format.</param>
    /// <returns>A localized, descriptive title for the category.</returns>
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

    /// <summary>
    /// Gets a concise summary description of the category's evaluation focus.
    /// </summary>
    /// <param name="category">The category type to inspect.</param>
    /// <returns>A short description of skills assessed in this category.</returns>
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

    /// <summary>
    /// Gets the Bootstrap icon class associated with the category.
    /// </summary>
    /// <param name="category">The category type to inspect.</param>
    /// <returns>A Bootstrap icon CSS class identifier.</returns>
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
