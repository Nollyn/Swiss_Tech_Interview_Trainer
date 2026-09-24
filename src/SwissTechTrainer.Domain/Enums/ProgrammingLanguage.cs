namespace SwissTechTrainer.Domain.Enums;

/// <summary>
/// Defines the supported backend programming languages for technical interview challenges and AI evaluations.
/// </summary>
public enum ProgrammingLanguage
{
    /// <summary>
    /// C# (.NET 10 / CLR ecosystem).
    /// </summary>
    CSharp = 1,

    /// <summary>
    /// Python (CPython 3.12+ / asyncio / GIL ecosystem).
    /// </summary>
    Python = 2,

    /// <summary>
    /// Java (OpenJDK 21+ / JVM / modern concurrency).
    /// </summary>
    Java = 3,

    /// <summary>
    /// Rust (Rust 2024 / ownership / borrow checker / zero-cost abstractions).
    /// </summary>
    Rust = 4,

    /// <summary>
    /// Go (Golang 1.23+ / goroutines / channels / standard library).
    /// </summary>
    Go = 5,

    /// <summary>
    /// Node.js (V8 / TypeScript & JavaScript / event loop ecosystem).
    /// </summary>
    NodeJs = 6
}

/// <summary>
/// Extension methods for formatting and configuring <see cref="ProgrammingLanguage"/>.
/// </summary>
public static class ProgrammingLanguageExtensions
{
    /// <summary>
    /// Gets the human-readable display name for the programming language.
    /// </summary>
    public static string GetDisplayName(this ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.CSharp => "C# (.NET)",
        ProgrammingLanguage.Python => "Python",
        ProgrammingLanguage.Java => "Java",
        ProgrammingLanguage.Rust => "Rust",
        ProgrammingLanguage.Go => "Go",
        ProgrammingLanguage.NodeJs => "Node.js",
        _ => language.ToString()
    };

    /// <summary>
    /// Gets the primary source code file extension associated with the language.
    /// </summary>
    public static string GetFileExtension(this ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.CSharp => ".cs",
        ProgrammingLanguage.Python => ".py",
        ProgrammingLanguage.Java => ".java",
        ProgrammingLanguage.Rust => ".rs",
        ProgrammingLanguage.Go => ".go",
        ProgrammingLanguage.NodeJs => ".js",
        _ => ".txt"
    };

    /// <summary>
    /// Gets the markdown/prism code fence identifier for the language.
    /// </summary>
    public static string GetCodeFenceTag(this ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.CSharp => "csharp",
        ProgrammingLanguage.Python => "python",
        ProgrammingLanguage.Java => "java",
        ProgrammingLanguage.Rust => "rust",
        ProgrammingLanguage.Go => "go",
        ProgrammingLanguage.NodeJs => "javascript",
        _ => "text"
    };

    /// <summary>
    /// Gets the Bootstrap/Devicon icon identifier or CSS class for UI presentation.
    /// </summary>
    public static string GetBadgeClass(this ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.CSharp => "bg-purple text-white",
        ProgrammingLanguage.Python => "bg-warning text-dark",
        ProgrammingLanguage.Java => "bg-danger text-white",
        ProgrammingLanguage.Rust => "bg-secondary text-white",
        ProgrammingLanguage.Go => "bg-info text-dark",
        ProgrammingLanguage.NodeJs => "bg-success text-white",
        _ => "bg-dark text-light"
    };
}
