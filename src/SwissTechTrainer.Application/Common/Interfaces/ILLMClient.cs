using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Application.Common.Interfaces;

/// <summary>
/// Abstraction interface (Port) for interacting with Large Language Model inference providers (Groq, Ollama, Deterministic Mock).
/// </summary>
public interface ILLMClient
{
    /// <summary>
    /// Gets the human-readable identifier of the active LLM provider strategy (e.g. "Groq Cloud (llama-3.3-70b)", "Ollama (Llama-3)", "DeterministicMock").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates a new dynamic technical exercise with structured JSON constraints and negative context prevention.
    /// </summary>
    /// <param name="context">The generation context containing category, level, and previous exercise reference.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A structured <see cref="GeneratedExerciseDto"/> representation.</returns>
    Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default);

    /// <summary>
    /// Evaluates a candidate's submission against category rubrics, producing per-criterion scores with cited evidence and code diff suggestions.
    /// </summary>
    /// <param name="context">The evaluation prompt context containing problem statement, rubrics, and submitted code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A structured <see cref="LlmEvaluationResponseDto"/> representation.</returns>
    Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default);
}
