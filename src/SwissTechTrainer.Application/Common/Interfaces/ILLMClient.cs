using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Application.Common.Interfaces;

public interface ILLMClient
{
    string ProviderName { get; }
    Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default);
    Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default);
}
