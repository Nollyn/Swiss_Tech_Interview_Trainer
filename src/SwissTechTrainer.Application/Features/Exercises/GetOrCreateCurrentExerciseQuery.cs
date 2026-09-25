using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

/// <summary>
/// Query to retrieve the candidate's currently active exercise for a category and programming language, or synthesize a new one if not yet initialized or already passed.
/// </summary>
/// <param name="Category">The interview category dimension.</param>
/// <param name="Language">The target programming language, defaulting to C#.</param>
/// <param name="UserId">Optional user ID filter, defaulting to current ambient user.</param>
public sealed record GetOrCreateCurrentExerciseQuery(CategoryType Category, ProgrammingLanguage Language = ProgrammingLanguage.CSharp, Guid? UserId = null) : IRequest<ExerciseDto>;

/// <summary>
/// Handler that ensures idempotent exercise delivery across page refreshes while serving new AI exercises on progression.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="llmClient">The LLM provider client.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class GetOrCreateCurrentExerciseQueryHandler(
    IApplicationDbContext context,
    ILLMClient llmClient,
    ICurrentUserService currentUserService,
    IExerciseLocalizationService exerciseLocalizer) : IRequestHandler<GetOrCreateCurrentExerciseQuery, ExerciseDto>
{
    /// <summary>
    /// Handles resolving or generating the candidate's active exercise.
    /// </summary>
    /// <param name="request">The get-or-create query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="ExerciseDto"/> representing the current exercise.</returns>
    public async Task<ExerciseDto> Handle(GetOrCreateCurrentExerciseQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);
        var targetLanguage = request.Language;

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

        var progress = user.GetOrCreateProgress(request.Category, targetLanguage);
        var currentLevel = progress.CurrentLevel;

        // Find the latest exercise for this category, level, and language
        var latestExercise = await context.Exercises
            .Where(e => e.Category == request.Category && e.Level == currentLevel && e.Language == targetLanguage)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // If an exercise exists, check if user has passed it
        if (latestExercise != null)
        {
            var userSubmissions = await context.Submissions
                .Include(s => s.Evaluation)
                .Where(s => s.ExerciseId == latestExercise.Id && s.UserId == targetUserId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync(cancellationToken);

            var alreadyPassedThisExercise = userSubmissions.Any(s => s.Evaluation != null && s.Evaluation.PassedThreshold);

            // If user hasn't passed it yet, keep serving this exercise so reload doesn't wipe their exercise
            if (!alreadyPassedThisExercise)
            {
                return MapToDto(latestExercise, progress.HintModeActive, exerciseLocalizer);
            }
        }

        // Otherwise, generate a new exercise using LLM with negative context
        var genContext = new ExerciseGenerationContext
        {
            Category = request.Category,
            Language = targetLanguage,
            Level = currentLevel,
            PreviousExerciseTitle = latestExercise?.Title,
            PreviousExerciseDescription = latestExercise?.Description,
            IncludeHintModeContext = progress.HintModeActive
        };

        var generated = await llmClient.GenerateExerciseAsync(genContext, cancellationToken);

        var newExercise = Exercise.Create(
            category: request.Category,
            level: currentLevel,
            title: generated.Title,
            description: generated.Description,
            starterCode: generated.StarterCode,
            expectedOutputFormat: generated.ExpectedOutputFormat,
            hints: generated.Hints,
            isAiGenerated: true,
            previousExerciseReferenceId: latestExercise?.Id,
            language: targetLanguage
        );

        context.Exercises.Add(newExercise);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(newExercise, progress.HintModeActive, exerciseLocalizer);
    }

    private static ExerciseDto MapToDto(Exercise exercise, bool hintModeActive, IExerciseLocalizationService localizer) => new()
    {
        Id = exercise.Id,
        Category = exercise.Category,
        CategoryDisplayName = exercise.Category.GetDisplayName(exercise.Language),
        Language = exercise.Language,
        LanguageDisplayName = exercise.Language.GetDisplayName(),
        Level = exercise.Level,
        LevelLabel = exercise.Level.GetLabel(),
        Title = exercise.Title,
        LocalizedTitle = localizer.LocalizeTitle(exercise.Category, exercise.Level, exercise.Title),
        Description = exercise.Description,
        LocalizedDescription = localizer.LocalizeDescription(exercise.Category, exercise.Level, exercise.Description),
        StarterCode = exercise.StarterCode,
        ExpectedOutputFormat = exercise.ExpectedOutputFormat,
        Hints = exercise.Hints,
        LocalizedHints = localizer.LocalizeHints(exercise.Category, exercise.Level, exercise.Hints),
        HintModeActive = hintModeActive,
        IsAiGenerated = exercise.IsAIGenerated,
        CreatedAt = exercise.CreatedAt
    };
}
