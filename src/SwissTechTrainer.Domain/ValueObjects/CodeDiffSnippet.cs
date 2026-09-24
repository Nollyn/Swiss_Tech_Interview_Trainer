namespace SwissTechTrainer.Domain.ValueObjects;

public sealed record CodeDiffSnippet
{
    public string Title { get; init; } = string.Empty;
    public string OriginalCode { get; init; } = string.Empty;
    public string SuggestedCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;

    public CodeDiffSnippet() { }

    public CodeDiffSnippet(string title, string originalCode, string suggestedCode, string explanation)
    {
        Title = title ?? string.Empty;
        OriginalCode = originalCode ?? string.Empty;
        SuggestedCode = suggestedCode ?? string.Empty;
        Explanation = explanation ?? string.Empty;
    }
}
