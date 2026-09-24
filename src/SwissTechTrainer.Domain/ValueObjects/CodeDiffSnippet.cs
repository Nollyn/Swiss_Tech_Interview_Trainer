namespace SwissTechTrainer.Domain.ValueObjects;

/// <summary>
/// Represents an actionable before-and-after code diff recommendation generated during evaluation.
/// </summary>
public sealed record CodeDiffSnippet
{
    /// <summary>
    /// Gets the title describing the refactoring opportunity (e.g., "Extract Interface & Decouple Dependencies").
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the candidate's original code snippet that contained the smell, inefficiency, or violation.
    /// </summary>
    public string OriginalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optimized, production-grade refactored code recommendation.
    /// </summary>
    public string SuggestedCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the architectural reasoning explaining why the suggested change improves quality.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeDiffSnippet"/> record. Parameterless constructor for serialization.
    /// </summary>
    public CodeDiffSnippet() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeDiffSnippet"/> record with validation.
    /// </summary>
    /// <param name="title">The descriptive title of the suggestion.</param>
    /// <param name="originalCode">The original code snippet.</param>
    /// <param name="suggestedCode">The suggested refactored snippet.</param>
    /// <param name="explanation">The explanation of the architectural benefit.</param>
    public CodeDiffSnippet(string title, string originalCode, string suggestedCode, string explanation)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Code Improvement Suggestion" : title.Trim();
        OriginalCode = originalCode?.Trim() ?? string.Empty;
        SuggestedCode = suggestedCode?.Trim() ?? string.Empty;
        Explanation = explanation?.Trim() ?? string.Empty;
    }
}
