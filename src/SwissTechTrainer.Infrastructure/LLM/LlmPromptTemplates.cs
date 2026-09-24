using System.Text.Json;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;

namespace SwissTechTrainer.Infrastructure.LLM;

/// <summary>
/// Provides structured system and user prompt builders for LLM exercise synthesis and candidate code evaluation.
/// </summary>
public static class LlmPromptTemplates
{
    /// <summary>
    /// Builds the system prompt enforcing JSON schema, role positioning, and Swiss market criteria for exercise generation.
    /// </summary>
    /// <returns>The formatted system prompt string.</returns>
    public static string BuildExerciseGenerationSystemPrompt()
    {
        return """
You are a Principal Staff Software Architect and Technical Hiring Bar Raiser for top Swiss tech companies and private banks in Zurich (SIX Group, UBS, Avaloq, Swissquote, Zühlke).
Your task is to generate an authentic, senior-level technical interview exercise for a Senior .NET Developer / Tech Lead candidate.

CRITICAL INSTRUCTIONS:
1. Output MUST be ONLY valid JSON adhering strictly to the JSON schema specified below.
2. No markdown wrapper outside the JSON (no ```json ... ```).
3. The exercise must reflect realistic Swiss engineering practices (clean architecture, zero-allocation considerations, resilience, testability, high regulatory standards).
4. Do NOT repeat or duplicate the previous exercise provided in the negative context.

JSON SCHEMA:
{
  "title": "Clear, professional exercise title",
  "description": "Full problem specification in Markdown with context, requirements, constraints, and acceptance criteria.",
  "starterCode": "C# starter code / boilerplate with interfaces and TODO comments, or Markdown architecture template.",
  "expectedOutputFormat": "C# Source File (.cs) or Markdown Justification",
  "hints": "Progressive diagnostic hints to assist candidates in hint mode."
}
""";
    }

    /// <summary>
    /// Builds the user prompt containing target category, level, rubrics, negative context, and hint mode triggers.
    /// </summary>
    /// <param name="context">The exercise generation context.</param>
    /// <returns>The formatted user prompt string.</returns>
    public static string BuildExerciseGenerationUserPrompt(ExerciseGenerationContext context)
    {
        var categoryName = context.Category.GetDisplayName();
        var levelLabel = context.Level.GetLabel();
        var rubrics = CategoryRubricCatalog.GetRubricForCategory(context.Category);
        string rubricText = string.Join("\n", rubrics.Select(r => $"- {r.Name} ({r.Weight}%): {r.Description}"));

        string negativeContext = string.Empty;
        if (!string.IsNullOrWhiteSpace(context.PreviousExerciseTitle))
        {
            negativeContext = $"""

NEGATIVE CONTEXT (DO NOT DUPLICATE THIS RECENT EXERCISE):
Previous Title: {context.PreviousExerciseTitle}
Previous Description Summary: {context.PreviousExerciseDescription?[..Math.Min(200, context.PreviousExerciseDescription.Length)]}
You MUST generate a completely distinct technical problem for this category and level.
""";
        }

        string hintModeText = context.IncludeHintModeContext
            ? "\nNOTE: The candidate has struggled on previous attempts. Ensure the 'hints' field contains thorough diagnostic questions and architectural hints."
            : string.Empty;

        return $"""
Generate a new technical interview exercise:
Category: {categoryName}
Difficulty: {levelLabel} (Level {(int)context.Level} of 5)

Evaluation Rubric that will be applied to candidate answers:
{rubricText}
{negativeContext}
{hintModeText}

Provide only the valid JSON response.
""";
    }

    /// <summary>
    /// Builds the system prompt for candidate submission evaluation following the Evaluator-Optimizer pattern.
    /// </summary>
    /// <returns>The formatted evaluator system prompt string.</returns>
    public static string BuildEvaluationSystemPrompt()
    {
        return """
You are a strict, objective, and constructive Principal Staff Software Architect conducting a senior technical interview assessment for the Zurich/Swiss tech market.
Your evaluation MUST follow the Evaluator-Optimizer methodology:
1. Evaluate each criterion separately on a scale of 0 to 100 based on cited evidence from the candidate's code.
2. Provide exact code evidence citations (referencing specific methods, allocations, or anti-patterns in the candidate's submission).
3. Provide concrete before/after code refactoring suggestions with syntax-highlighted snippets.
4. Output MUST be ONLY valid JSON matching the schema below. Do NOT calculate a total final score; your role is solely to score each individual criterion accurately and provide actionable feedback.

JSON SCHEMA:
{
  "criteria": [
    {
      "name": "Criterion Name",
      "score": 85.0,
      "evidence": "Exact snippet or citation from submitted code",
      "rationale": "Clear justification for why this score was awarded"
    }
  ],
  "suggestions": [
    {
      "title": "Specific Refactoring / Improvement Title",
      "originalCode": "Candidate snippet that has an issue",
      "suggestedCode": "Production-grade, idiomatic corrected snippet",
      "explanation": "Why this change improves performance, SOLID adherence, or resilience"
    }
  ],
  "generalFeedback": "Comprehensive, encouraging, yet rigorous feedback in Markdown format.",
  "modelUsed": "Model Identifier"
}
""";
    }

    /// <summary>
    /// Builds the user prompt for evaluating candidate code against rubrics and starter code.
    /// </summary>
    /// <param name="context">The evaluation prompt context.</param>
    /// <returns>The formatted evaluator user prompt string.</returns>
    public static string BuildEvaluationUserPrompt(EvaluationPromptContext context)
    {
        var categoryName = context.Category.GetDisplayName();
        var rubrics = CategoryRubricCatalog.GetRubricForCategory(context.Category);
        string rubricCriteriaList = string.Join("\n", rubrics.Select(r => $"- \"{r.Name}\" (Weight: {r.Weight}%): {r.Description}"));

        return $"""
Please evaluate the following candidate submission:

Category: {categoryName}
Difficulty Level: {context.Level.GetLabel()}
Exercise Title: {context.ExerciseTitle}

Exercise Description:
{context.ExerciseDescription}

Starter Code:
```csharp
{context.StarterCode}
```

Candidate Submitted Code:
```csharp
{context.SubmittedCode}
```

Additional Notes from Candidate:
{context.AdditionalNotes}

CRITERIA TO EVALUATE (You must evaluate each of these exact {rubrics.Count} criteria):
{rubricCriteriaList}

Provide only the valid JSON response adhering strictly to the schema.
""";
    }
}
