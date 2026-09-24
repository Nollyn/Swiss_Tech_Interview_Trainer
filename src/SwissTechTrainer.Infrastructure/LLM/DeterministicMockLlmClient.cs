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
        string categoryName = context.Category.GetDisplayName(context.Language);
        string langName = context.Language.GetDisplayName();
        string ext = context.Language.GetFileExtension();

        string starterCode = context.Language switch
        {
            ProgrammingLanguage.Python => """
# SwissTech Solutions - Python Senior Track
import asyncio
from typing import Any, Dict, Optional

class ProductionSolution:
    # Production-grade solution satisfying Swiss financial engineering standards.
    async def execute_async(self) -> None:
        # TODO: Implement senior-level solution with comprehensive edge case coverage
        await asyncio.sleep(0)
""",
            ProgrammingLanguage.Java => """
package ch.swisstech.solutions;

import java.util.concurrent.CompletableFuture;

public class ProductionSolution {
    /**
     * Executes production-grade solution satisfying Swiss financial engineering standards.
     */
    public CompletableFuture<Void> executeAsync() {
        // TODO: Implement senior-level solution with comprehensive edge case coverage
        return CompletableFuture.completedFuture(null);
    }
}
""",
            ProgrammingLanguage.Rust => """
//! SwissTech Solutions - Rust Senior Track

pub struct ProductionSolution;

impl ProductionSolution {
    /// Executes production-grade solution with zero-cost abstractions and fearless concurrency.
    pub async fn execute_async(&self) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
        // TODO: Implement senior-level solution with comprehensive edge case coverage
        Ok(())
    }
}
""",
            ProgrammingLanguage.Go => """
package main

import (
	"context"
)

// ProductionSolution represents a senior-level Go component.
type ProductionSolution struct{}

// ExecuteAsync runs the production workload with context propagation.
func (s *ProductionSolution) ExecuteAsync(ctx context.Context) error {
	// TODO: Implement senior-level solution with comprehensive edge case coverage
	return nil
}
""",
            ProgrammingLanguage.NodeJs => """
// SwissTech Solutions - Node.js / TypeScript Senior Track

export class ProductionSolution {
  /**
   * Executes production-grade asynchronous workload.
   */
  public async executeAsync(): Promise<void> {
    // TODO: Implement senior-level solution with comprehensive edge case coverage
    await Promise.resolve();
  }
}
""",
            _ => """
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
"""
        };

        var exercise = new GeneratedExerciseDto
        {
            Title = $"{categoryName} - Swiss Enterprise Challenge (Level {levelNum})",
            Description = $"""
### Swiss Financial Services & Tech Lead Track: {categoryName} (Level {levelNum})

#### Business Context
In Zurich high-concurrency systems, robust architectural decisions, idioms in {langName}, and performance under regulatory constraints are critical.

#### Problem Specification
Design and implement a production-grade component in {langName} for Level {levelNum} requirements in {categoryName}.

#### Constraints
- Adhere strictly to clean architecture, {langName} idioms, and defensive programming.
- Ensure optimal time and space complexity with zero unnecessary allocations in hot paths.
- Handle edge cases gracefully (null/nil/empty inputs, cancellation/context propagation, transient downstream failures).
""",
            StarterCode = starterCode.Trim(),
            ExpectedOutputFormat = $"{langName} Source File ({ext}) or Markdown Justification",
            Hints = context.IncludeHintModeContext
                ? $"Diagnostic Hint for {langName}: Decompose the problem into clean modular interfaces/types. Avoid unnecessary memory allocations and handle errors defensively."
                : $"Standard Hint for {langName}: Focus on clean separation of concerns and deterministic error handling."
        };

        return Task.FromResult(exercise);
    }

    /// <inheritdoc />
    public Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        var officialRubrics = CategoryRubricCatalog.GetRubricForCategory(context.Category, context.Language);
        var criteria = new List<LlmEvaluationCriterionDto>();

        // Realistic heuristic analysis based on code content to simulate high-fidelity LLM grading
        string code = context.SubmittedCode ?? string.Empty;
        bool hasGoodLength = code.Length > 200;
        bool hasDefensiveChecks = code.Contains("throw", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("null", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("nil", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("None", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("Argument", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("Result", StringComparison.OrdinalIgnoreCase) ||
                                  code.Contains("err !=", StringComparison.OrdinalIgnoreCase);

        bool hasModernFeatures = code.Contains("async", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("Span", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("record", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("impl", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("goroutine", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("def ", StringComparison.OrdinalIgnoreCase) ||
                                 code.Contains("pattern", StringComparison.OrdinalIgnoreCase);

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

        var suggestions = GetSuggestionsForLanguage(context.Language);

        var response = new LlmEvaluationResponseDto
        {
            Criteria = criteria,
            Suggestions = suggestions,
            GeneralFeedback = baseScore >= 90.0
                ? $"### Excellent Senior Submission ({context.Language.GetDisplayName()})\n\nYour solution demonstrates strong technical maturity, clean separation of concerns, and robust error handling aligned with Zurich senior tech lead standards. Proceed to the next level challenge."
                : $"### Constructive Feedback ({context.Language.GetDisplayName()})\n\nYour submission is a solid attempt, but falls slightly below the 90% mastery threshold required for level progression. Review the cited evidence and before/after code refactorings, then retry or explore a similar variant.",
            ModelUsed = ProviderName
        };

        return Task.FromResult(response);
    }

    private static List<LlmCodeSuggestionDto> GetSuggestionsForLanguage(ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.Python =>
        [
            new()
            {
                Title = "Adopt Context Manager & Type Annotations",
                OriginalCode = "def process(data):\n    f = open('data.txt')\n    return f.read()",
                SuggestedCode = "def process(data: str) -> str:\n    with open('data.txt', 'r', encoding='utf-8') as f:\n        return f.read()",
                Explanation = "Prevents file descriptor leaks by using a deterministic context manager and enforces PEP 484 type annotations."
            }
        ],
        ProgrammingLanguage.Java =>
        [
            new()
            {
                Title = "Use Try-With-Resources & Pattern Matching",
                OriginalCode = "InputStream is = socket.getInputStream(); ... is.close();",
                SuggestedCode = "try (InputStream is = socket.getInputStream()) {\n    // auto-closed safely\n}",
                Explanation = "Guarantees resource release upon exceptions according to modern Java clean code standards."
            }
        ],
        ProgrammingLanguage.Rust =>
        [
            new()
            {
                Title = "Avoid Cloning and Use Borrow Slicing",
                OriginalCode = "let copy = text.clone();\nparse(&copy);",
                SuggestedCode = "let slice: &str = &text;\nparse(slice);",
                Explanation = "Eliminates unnecessary heap reallocation by leveraging Rust reference borrowing."
            }
        ],
        ProgrammingLanguage.Go =>
        [
            new()
            {
                Title = "Propagate Context for Cooperative Cancellation",
                OriginalCode = "func (s *Service) FetchData() (*Data, error)",
                SuggestedCode = "func (s *Service) FetchData(ctx context.Context) (*Data, error)",
                Explanation = "Enables graceful timeout handling and cancellation across distributed Go microservices."
            }
        ],
        ProgrammingLanguage.NodeJs =>
        [
            new()
            {
                Title = "Avoid Unhandled Rejections with Async Flow",
                OriginalCode = "fs.readFile(path, (err, data) => { ... });",
                SuggestedCode = "const data = await fs.promises.readFile(path, 'utf8');",
                Explanation = "Modern async/await with Promise-based fs prevents callback hell and integrates with central error middleware."
            }
        ],
        _ =>
        [
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
        ]
    };
}
