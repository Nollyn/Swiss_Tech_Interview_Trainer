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

public sealed record SubmitExerciseCommand : IRequest<EvaluationResultDto>
{
    public Guid ExerciseId { get; init; }
    public string SubmittedCode { get; init; } = string.Empty;
    public string AdditionalNotes { get; init; } = string.Empty;
    public SubmissionType SubmissionType { get; init; } = SubmissionType.SingleFile;
    public Guid? UserId { get; init; }
}

public sealed class SubmitExerciseCommandValidator : AbstractValidator<SubmitExerciseCommand>
{
    public SubmitExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("Exercise ID is required.");
        RuleFor(x => x.SubmittedCode).NotEmpty().WithMessage("Submitted code cannot be empty.");
    }
}

public sealed class SubmitExerciseCommandHandler : IRequestHandler<SubmitExerciseCommand, EvaluationResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILLMClient _llmClient;
    private readonly ICurrentUserService _currentUserService;

    public SubmitExerciseCommandHandler(
        IApplicationDbContext context,
        ILLMClient llmClient,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _llmClient = llmClient;
        _currentUserService = currentUserService;
    }

    public async Task<EvaluationResultDto> Handle(SubmitExerciseCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var targetUserId = request.UserId ?? await _currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var user = await _context.UserProfiles
            .Include(u => u.Progresses)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user == null)
        {
            user = new UserProfile(_currentUserService.Username, "candidate@swisstech.ch");
            _context.UserProfiles.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exercise with ID '{request.ExerciseId}' was not found.");

        // Create submission entity
        var submission = new Submission(
            exerciseId: exercise.Id,
            userId: user.Id,
            submittedCode: request.SubmittedCode,
            additionalNotes: request.AdditionalNotes,
            submissionType: request.SubmissionType
        );

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);

        // Prepare Evaluator-Optimizer prompt context
        var evalContext = new EvaluationPromptContext
        {
            Category = exercise.Category,
            Level = exercise.Level,
            ExerciseTitle = exercise.Title,
            ExerciseDescription = exercise.Description,
            StarterCode = exercise.StarterCode,
            SubmittedCode = request.SubmittedCode,
            AdditionalNotes = request.AdditionalNotes
        };

        // Call LLM
        var llmResponse = await _llmClient.EvaluateSubmissionAsync(evalContext, cancellationToken);

        // Map and validate criteria against official domain rubric
        var officialRubric = CategoryRubricCatalog.GetRubricForCategory(exercise.Category);
        var criteriaScores = new List<CriterionScore>();

        foreach (var rubricItem in officialRubric)
        {
            var matchedLlmCriterion = llmResponse.Criteria
                .FirstOrDefault(c => c.Name.Equals(rubricItem.Name, StringComparison.OrdinalIgnoreCase))
                ?? llmResponse.Criteria.FirstOrDefault(c => c.Name.Contains(rubricItem.Name[..Math.Min(10, rubricItem.Name.Length)], StringComparison.OrdinalIgnoreCase));

            double score = matchedLlmCriterion != null ? Math.Clamp(matchedLlmCriterion.Score, 0.0, 100.0) : 75.0;
            string evidence = matchedLlmCriterion?.Evidence ?? "Evaluated against standard criteria";
            string rationale = matchedLlmCriterion?.Rationale ?? rubricItem.Description;

            criteriaScores.Add(new CriterionScore(
                name: rubricItem.Name,
                weight: rubricItem.Weight,
                score: score,
                evidence: evidence,
                rationale: rationale
            ));
        }

        var codeSuggestions = (llmResponse.Suggestions ?? new List<LlmCodeSuggestionDto>())
            .Select(s => new CodeDiffSnippet(s.Title, s.OriginalCode, s.SuggestedCode, s.Explanation))
            .ToList();

        stopwatch.Stop();

        // Create Evaluation entity
        var evaluation = new Evaluation(submission.Id);
        evaluation.Complete(
            criteria: criteriaScores,
            suggestions: codeSuggestions,
            generalFeedback: llmResponse.GeneralFeedback,
            llmModelUsed: string.IsNullOrWhiteSpace(llmResponse.ModelUsed) ? _llmClient.ProviderName : llmResponse.ModelUsed,
            durationMs: stopwatch.ElapsedMilliseconds
        );

        submission.AttachEvaluation(evaluation);
        _context.Evaluations.Add(evaluation);

        // Apply domain progression specification
        var progress = user.GetOrCreateProgress(exercise.Category);
        var progressionResult = ProgressionSpecification.EvaluateProgression(progress, evaluation.DeterministicScore);

        // Update progress entity
        progress.RecordAttempt(evaluation.DeterministicScore, evaluation.PassedThreshold);
        user.TouchActivity();

        await _context.SaveChangesAsync(cancellationToken);

        return new EvaluationResultDto
        {
            SubmissionId = submission.Id,
            ExerciseId = exercise.Id,
            Category = exercise.Category,
            CategoryDisplayName = exercise.Category.GetDisplayName(),
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
