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
    /// CLR/language execution internals, memory management, and asynchronous concurrency.
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
    public static string GetDisplayName(this CategoryType category) => category.GetDisplayName(ProgrammingLanguage.CSharp);

    /// <summary>
    /// Gets the human-readable display title for the specified interview category adapted to the candidate's chosen programming language.
    /// </summary>
    /// <param name="category">The category type to format.</param>
    /// <param name="language">The selected programming language.</param>
    /// <returns>A localized, descriptive title tailored to the language.</returns>
    public static string GetDisplayName(this CategoryType category, ProgrammingLanguage language) => category switch
    {
        CategoryType.Coding => $"Coding / Algorithms ({language.GetDisplayName()})",
        CategoryType.SystemDesign => "System Design",
        CategoryType.DotNetDeepDive => language switch
        {
            ProgrammingLanguage.CSharp => ".NET / C# Deep Dive",
            ProgrammingLanguage.Python => "Python Deep Dive",
            ProgrammingLanguage.Java => "Java Deep Dive",
            ProgrammingLanguage.Rust => "Rust Deep Dive",
            ProgrammingLanguage.Go => "Go Deep Dive",
            ProgrammingLanguage.NodeJs => "Node.js Deep Dive",
            _ => $"{language.GetDisplayName()} Deep Dive"
        },
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
    public static string GetShortDescription(this CategoryType category) => category.GetShortDescription(ProgrammingLanguage.CSharp);

    /// <summary>
    /// Gets a concise summary description of the category's evaluation focus adapted to the target language.
    /// </summary>
    /// <param name="category">The category type to inspect.</param>
    /// <param name="language">The selected programming language.</param>
    /// <returns>A short description tailored to the language.</returns>
    public static string GetShortDescription(this CategoryType category, ProgrammingLanguage language) => category switch
    {
        CategoryType.Coding => $"Pragmatic algorithms, time/space complexity, and idiomatic {language.GetDisplayName()} structures.",
        CategoryType.SystemDesign => "Distributed service design, resilience, and trade-off justifications.",
        CategoryType.DotNetDeepDive => language switch
        {
            ProgrammingLanguage.CSharp => "Memory management, async/await internals, GC, and CLR performance.",
            ProgrammingLanguage.Python => "GIL, asyncio event loop, generators/decorators, and memory management.",
            ProgrammingLanguage.Java => "JVM internals, Garbage Collectors (G1/ZGC), and JMM concurrency.",
            ProgrammingLanguage.Rust => "Ownership, lifetimes, borrow checker, and zero-cost abstractions.",
            ProgrammingLanguage.Go => "Goroutines, channels, CSP concurrency, escape analysis, and interfaces.",
            ProgrammingLanguage.NodeJs => "V8 internals, event loop phases, non-blocking I/O, and stream processing.",
            _ => $"Internals, memory models, and concurrency in {language.GetDisplayName()}."
        },
        CategoryType.CleanCode => $"Refactoring code smells, SOLID/idiomatic principles, and technical debt in {language.GetDisplayName()}.",
        CategoryType.ApiDesign => "REST/gRPC contracts, idempotency, retry policies, and circuit breakers.",
        CategoryType.Testing => $"Unit, integration, and mutation testing in {language.GetDisplayName()} using strict TDD.",
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
