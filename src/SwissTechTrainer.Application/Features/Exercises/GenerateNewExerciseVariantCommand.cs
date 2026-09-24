using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

/// <summary>
/// Command to request dynamic LLM generation of a new exercise variant for the candidate's current category and level.
/// </summary>
/// <param name="Category">The interview category dimension.</param>
/// <param name="UserId">Optional user ID filter, defaulting to current ambient user.</param>
public sealed record GenerateNewExerciseVariantCommand(CategoryType Category, Guid? UserId = null) : IRequest<ExerciseDto>;

/// <summary>
/// Handler responsible for orchestrating negative-context prompt assembly, LLM exercise generation, and persistence.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="llmClient">The LLM provider client.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class GenerateNewExerciseVariantCommandHandler(
    IApplicationDbContext context,
    ILLMClient llmClient,
    ICurrentUserService currentUserService) : IRequestHandler<GenerateNewExerciseVariantCommand, ExerciseDto>
{
    /// <summary>
    /// Handles generating a fresh non-repetitive exercise variant using negative prompting against prior exercises.
    /// </summary>
    /// <param name="request">The generation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="ExerciseDto"/> representing the newly created exercise.</returns>
    public async Task<ExerciseDto> Handle(GenerateNewExerciseVariantCommand request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var user = await context.UserProfiles
            .Include(u => u.Progresses)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user == null)
        {
            user = UserProfile.Create(currentUserService.Username, "candidate@swisstech.ch");
            context.UserProfiles.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        var progress = user.GetOrCreateProgress(request.Category);
        var currentLevel = progress.CurrentLevel;

        var latestExercise = await context.Exercises
            .Where(e => e.Category == request.Category && e.Level == currentLevel)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var genContext = new ExerciseGenerationContext
        {
            Category = request.Category,
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
            previousExerciseReferenceId: latestExercise?.Id
        );

        context.Exercises.Add(newExercise);
        await context.SaveChangesAsync(cancellationToken);

        return new ExerciseDto
        {
            Id = newExercise.Id,
            Category = newExercise.Category,
            CategoryDisplayName = newExercise.Category.GetDisplayName(),
            Level = newExercise.Level,
            LevelLabel = newExercise.Level.GetLabel(),
            Title = newExercise.Title,
            Description = newExercise.Description,
            StarterCode = newExercise.StarterCode,
            ExpectedOutputFormat = newExercise.ExpectedOutputFormat,
            Hints = newExercise.Hints,
            HintModeActive = progress.HintModeActive,
            IsAiGenerated = newExercise.IsAIGenerated,
            CreatedAt = newExercise.CreatedAt
        };
    }
}
