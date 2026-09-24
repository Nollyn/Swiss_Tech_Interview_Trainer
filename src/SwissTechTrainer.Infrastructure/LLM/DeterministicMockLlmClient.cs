using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;

namespace SwissTechTrainer.Infrastructure.LLM;

/// <summary>
/// High-fidelity deterministic simulator used in test suites and offline demo environments.
/// </summary>
public class DeterministicMockLlmClient : ILLMClient
{
    /// <inheritdoc />
    public string ProviderName => "Deterministic-Mock-LLM (Offline/Test Engine)";

    /// <inheritdoc />
    public Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default)
    {
        int levelNum = (int)context.Level;
        string categoryName = context.Category.GetDisplayName();

        var exercise = new GeneratedExerciseDto
        {
            Title = $"{categoryName} - Swiss Enterprise Challenge (Level {levelNum})",
            Description = $"""
### Swiss Financial Services & Tech Lead Track: {categoryName} (Level {levelNum})

#### Business Context
In Zurich high-concurrency systems, robust architectural decisions and performance under regulatory constraints are critical.

#### Problem Specification
Design and implement a production-grade component for Level {levelNum} requirements in {categoryName}.

#### Constraints
- Adhere strictly to clean architecture, SOLID principles, and defensive programming.
- Ensure optimal time and space complexity with zero unnecessary allocations in hot paths.
- Handle edge cases gracefully (null/empty inputs, cancellation tokens, transient downstream failures).
""",
            StarterCode = """
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SwissTech.Solutions;

public class ProductionSolution
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        // TODO: Implement senior-level solution with comprehensive edge case coverage
        await Task.Yield();
    }
}
""",
            ExpectedOutputFormat = "C# Source File (.cs) or Markdown Justification",
            Hints = context.IncludeHintModeContext
                ? "Diagnostic Hint: Decompose the problem into separate single-responsibility interfaces. Avoid premature heap allocation by using value types or spans where appropriate."
                : "Standard Hint: Focus on clean separation of concerns and deterministic error handling."
        };

        return Task.FromResult(exercise);
    }

    /// <inheritdoc />
    public Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        var officialRubrics = CategoryRubricCatalog.GetRubricForCategory(context.Category);
        var criteria = new List<LlmEvaluationCriterionDto>();

        // Realistic heuristic analysis based on code content to simulate high-fidelity LLM grading
        string code = context.SubmittedCode ?? string.Empty;
        bool hasGoodLength = code.Length > 200;
        bool hasDefensiveChecks = code.Contains("throw", StringComparison.OrdinalIgnoreCase) || code.Contains("null", StringComparison.OrdinalIgnoreCase) || code.Contains("Argument", StringComparison.OrdinalIgnoreCase);
        bool hasModernFeatures = code.Contains("async", StringComparison.OrdinalIgnoreCase) || code.Contains("Span", StringComparison.OrdinalIgnoreCase) || code.Contains("record", StringComparison.OrdinalIgnoreCase) || code.Contains("pattern", StringComparison.OrdinalIgnoreCase);

        double baseScore = hasGoodLength ? 92.0 : 78.0;
        if (hasDefensiveChecks) baseScore += 3.0;
        if (hasModernFeatures) baseScore += 2.0;
        baseScore = Math.Clamp(baseScore, 65.0, 97.0);

        foreach (var rubric in officialRubrics)
        {
            double criterionScore = baseScore;
            string evidence;
            string rationale;

            if (baseScore >= 90.0)
            {
                evidence = $"Code demonstrated strong adherence to {rubric.Name} with clean structure and robust edge-case handling.";
                rationale = $"Exemplary senior implementation satisfying Swiss banking standards for {rubric.Name}.";
            }
            else
            {
                evidence = $"Method decomposition and defensive boundaries for {rubric.Name} need further refinement.";
                rationale = $"Score reflects opportunities to strengthen modularity and resilience for {rubric.Name}.";
            }

            criteria.Add(new LlmEvaluationCriterionDto
            {
                Name = rubric.Name,
                Weight = rubric.Weight,
                Score = criterionScore,
                Evidence = evidence,
                Rationale = rationale
            });
        }

        var suggestions = new List<LlmCodeSuggestionDto>
        {
            new()
            {
                Title = "Adopt ReadOnlySpan / Memory Slicing",
                OriginalCode = "var parts = text.Split(';');",
                SuggestedCode = "ReadOnlySpan<char> span = text.AsSpan();\nint idx = span.IndexOf(';');",
                Explanation = "Replaces array allocations on heap with zero-allocation span slicing in high-throughput hot paths."
            },
            new()
            {
                Title = "Add Explicit CancellationToken Propagation",
                OriginalCode = "public async Task ProcessAsync() { ... }",
                SuggestedCode = "public async Task ProcessAsync(CancellationToken cancellationToken = default) { ... }",
                Explanation = "Ensures clean cooperative cancellation in distributed ASP.NET Core pipelines."
            }
        };

        var response = new LlmEvaluationResponseDto
        {
            Criteria = criteria,
            Suggestions = suggestions,
            GeneralFeedback = baseScore >= 90.0
                ? "### Excellent Senior Submission\n\nYour solution demonstrates strong technical maturity, clean separation of concerns, and robust error handling aligned with Zurich senior tech lead standards. Proceed to the next level challenge."
                : "### Constructive Feedback\n\nYour submission is a solid attempt, but falls slightly below the 90% mastery threshold required for level progression. Review the cited evidence and before/after code refactorings, then retry or explore a similar variant.",
            ModelUsed = ProviderName
        };

        return Task.FromResult(response);
    }
}
