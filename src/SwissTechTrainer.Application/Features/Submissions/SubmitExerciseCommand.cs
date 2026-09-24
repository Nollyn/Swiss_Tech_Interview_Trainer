using System.Diagnostics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;
using SwissTechTrainer.Domain.Specifications;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Application.Features.Submissions;

/// <summary>
/// Command to submit candidate code or text for automated LLM evaluation, rubric breakdown, and progression evaluation.
/// </summary>
public sealed record SubmitExerciseCommand : IRequest<EvaluationResultDto>
{
    /// <summary>
    /// Gets the target exercise ID.
    /// </summary>
    public Guid ExerciseId { get; init; }

    /// <summary>
    /// Gets the candidate's submitted source code or justification text.
    /// </summary>
    public string SubmittedCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets optional candidate notes or architectural justifications.
    /// </summary>
    public string AdditionalNotes { get; init; } = string.Empty;

    /// <summary>
    /// Gets the format of the submission.
    /// </summary>
    public SubmissionType SubmissionType { get; init; } = SubmissionType.SingleFile;

    /// <summary>
    /// Gets the optional candidate user ID, defaulting to current ambient user.
    /// </summary>
    public Guid? UserId { get; init; }
}

/// <summary>
/// FluentValidation validator enforcing required fields for <see cref="SubmitExerciseCommand"/>.
/// </summary>
public sealed class SubmitExerciseCommandValidator : AbstractValidator<SubmitExerciseCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SubmitExerciseCommandValidator"/> with validation rules.
    /// </summary>
    public SubmitExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("Exercise ID is required.");
        RuleFor(x => x.SubmittedCode).NotEmpty().WithMessage("Submitted code cannot be empty.");
    }
}

/// <summary>
/// Handler that orchestrates the Evaluator-Optimizer pipeline, calls the LLM provider, computes deterministic grades, and advances candidate level if >= 90%.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="llmClient">The LLM provider client.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class SubmitExerciseCommandHandler(
    IApplicationDbContext context,
    ILLMClient llmClient,
    ICurrentUserService currentUserService) : IRequestHandler<SubmitExerciseCommand, EvaluationResultDto>
{
    /// <summary>
    /// Handles submission evaluation and progression gate calculation.
    /// </summary>
    /// <param name="request">The submission command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="EvaluationResultDto"/> containing scores, breakdown, and progression results.</returns>
    public async Task<EvaluationResultDto> Handle(SubmitExerciseCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var targetUserId = request.UserId ?? await currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var user = await context.UserProfiles
            .Include(u => u.Progresses)
            .Where(u => u.Id == targetUserId)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            user = UserProfile.Create(currentUserService.Username, "candidate@swisstech.ch");
            context.UserProfiles.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        var exercise = await context.Exercises
            .Where(e => e.Id == request.ExerciseId)
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Exercise with ID '{request.ExerciseId}' was not found.");

        // Create submission entity using factory
        var submission = Submission.Create(
            exerciseId: exercise.Id,
            userId: user.Id,
            submittedCode: request.SubmittedCode,
            additionalNotes: request.AdditionalNotes,
            submissionType: request.SubmissionType,
            language: exercise.Language
        );

        context.Submissions.Add(submission);
        await context.SaveChangesAsync(cancellationToken);

        // Prepare Evaluator-Optimizer prompt context
        var evalContext = new EvaluationPromptContext
        {
            Category = exercise.Category,
            Language = exercise.Language,
            Level = exercise.Level,
            ExerciseTitle = exercise.Title,
            ExerciseDescription = exercise.Description,
            StarterCode = exercise.StarterCode,
            SubmittedCode = request.SubmittedCode,
            AdditionalNotes = request.AdditionalNotes
        };

        // Call LLM
        var llmResponse = await llmClient.EvaluateSubmissionAsync(evalContext, cancellationToken);

        // Map and validate criteria against official domain rubric for this language
        var officialRubric = CategoryRubricCatalog.GetRubricForCategory(exercise.Category, exercise.Language);
        var criteriaScores = (from rubricItem in officialRubric
            let matchedLlmCriterion = llmResponse.Criteria.FirstOrDefault(c => c.Name.Equals(rubricItem.Name, StringComparison.OrdinalIgnoreCase)) ??
                                      llmResponse.Criteria.FirstOrDefault(c => c.Name.Contains(rubricItem.Name[..Math.Min(10, rubricItem.Name.Length)], StringComparison.OrdinalIgnoreCase))
            let score = matchedLlmCriterion != null ? Math.Clamp(matchedLlmCriterion.Score, 0.0, 100.0) : 75.0
            let evidence = matchedLlmCriterion?.Evidence ?? "Evaluated against standard criteria"
            let rationale = matchedLlmCriterion?.Rationale ?? rubricItem.Description
            select new CriterionScore(name: rubricItem.Name, weight: rubricItem.Weight, score: score, evidence: evidence, rationale: rationale)).ToList();

        var codeSuggestions = (llmResponse.Suggestions ?? [])
            .Select(s => new CodeDiffSnippet(s.Title, s.OriginalCode, s.SuggestedCode, s.Explanation))
            .ToList();

        stopwatch.Stop();

        // Create Evaluation entity using factory
        var evaluation = Evaluation.CreatePending(submission.Id, exercise.Language);
        evaluation.Complete(
            criteria: criteriaScores,
            suggestions: codeSuggestions,
            generalFeedback: llmResponse.GeneralFeedback,
            llmModelUsed: string.IsNullOrWhiteSpace(llmResponse.ModelUsed) ? llmClient.ProviderName : llmResponse.ModelUsed,
            durationMs: stopwatch.ElapsedMilliseconds
        );

        submission.AttachEvaluation(evaluation);
        context.Evaluations.Add(evaluation);

        // Apply domain progression specification isolated for this language track
        var progress = user.GetOrCreateProgress(exercise.Category, exercise.Language);
        var progressionResult = ProgressionSpecification.EvaluateProgression(progress, evaluation.DeterministicScore);

        // Update progress entity
        progress.RecordAttempt(evaluation.DeterministicScore, evaluation.PassedThreshold);
        user.TouchActivity();

        await context.SaveChangesAsync(cancellationToken);

        return new EvaluationResultDto
        {
            SubmissionId = submission.Id,
            ExerciseId = exercise.Id,
            Category = exercise.Category,
            Language = exercise.Language,
            CategoryDisplayName = exercise.Category.GetDisplayName(exercise.Language),
            Level = exercise.Level,
            LevelLabel = exercise.Level.GetLabel(),
            DeterministicScore = evaluation.DeterministicScore,
            PassedThreshold = evaluation.PassedThreshold,
            GeneralFeedback = evaluation.GeneralFeedback,
            CriteriaBreakdown = criteriaScores.Select(c => new CriterionEvaluationResultDto
            {
                Name = c.Name,
                Weight = c.Weight,
                Score = c.Score,
                Evidence = c.Evidence,
                Rationale = c.Rationale
            }).ToList(),
            CodeSuggestions = codeSuggestions.Select(s => new CodeSuggestionResultDto
            {
                Title = s.Title,
                OriginalCode = s.OriginalCode,
                SuggestedCode = s.SuggestedCode,
                Explanation = s.Explanation
            }).ToList(),
            LevelAdvanced = progressionResult.LevelAdvanced,
            NewLevel = progressionResult.NewLevel,
            ConsecutiveFailures = progress.ConsecutiveFailures,
            HintModeUnlocked = progress.HintModeActive,
            EvaluatedAt = evaluation.EvaluatedAt,
            ModelUsed = evaluation.LlmModelUsed,
            ProcessingDurationMs = evaluation.ProcessingDurationMs
        };
    }
}
