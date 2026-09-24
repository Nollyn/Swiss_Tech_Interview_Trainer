using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

public sealed record GetOrCreateCurrentExerciseQuery(CategoryType Category, Guid? UserId = null) : IRequest<ExerciseDto>;

public sealed class GetOrCreateCurrentExerciseQueryHandler : IRequestHandler<GetOrCreateCurrentExerciseQuery, ExerciseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILLMClient _llmClient;
    private readonly ICurrentUserService _currentUserService;

    public GetOrCreateCurrentExerciseQueryHandler(
        IApplicationDbContext context,
        ILLMClient llmClient,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _llmClient = llmClient;
        _currentUserService = currentUserService;
    }

    public async Task<ExerciseDto> Handle(GetOrCreateCurrentExerciseQuery request, CancellationToken cancellationToken)
    {
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

        var progress = user.GetOrCreateProgress(request.Category);
        var currentLevel = progress.CurrentLevel;

        // Find the latest exercise for this category and level
        var latestExercise = await _context.Exercises
            .Where(e => e.Category == request.Category && e.Level == currentLevel)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // If an exercise exists, check if user has passed it
        if (latestExercise != null)
        {
            var userSubmissions = await _context.Submissions
                .Include(s => s.Evaluation)
                .Where(s => s.ExerciseId == latestExercise.Id && s.UserId == targetUserId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync(cancellationToken);

            bool alreadyPassedThisExercise = userSubmissions.Any(s => s.Evaluation != null && s.Evaluation.PassedThreshold);

            // If user hasn't passed it yet, keep serving this exercise so reload doesn't wipe their exercise
            if (!alreadyPassedThisExercise)
            {
                return MapToDto(latestExercise, progress.HintModeActive);
            }
        }

        // Otherwise, generate a new exercise using LLM with negative context
        var genContext = new ExerciseGenerationContext
        {
            Category = request.Category,
            Level = currentLevel,
            PreviousExerciseTitle = latestExercise?.Title,
            PreviousExerciseDescription = latestExercise?.Description,
            IncludeHintModeContext = progress.HintModeActive
        };

        var generated = await _llmClient.GenerateExerciseAsync(genContext, cancellationToken);

        var newExercise = new Exercise(
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

        _context.Exercises.Add(newExercise);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(newExercise, progress.HintModeActive);
    }

    private static ExerciseDto MapToDto(Exercise exercise, bool hintModeActive) => new()
    {
        Id = exercise.Id,
        Category = exercise.Category,
        CategoryDisplayName = exercise.Category.GetDisplayName(),
        Level = exercise.Level,
        LevelLabel = exercise.Level.GetLabel(),
        Title = exercise.Title,
        Description = exercise.Description,
        StarterCode = exercise.StarterCode,
        ExpectedOutputFormat = exercise.ExpectedOutputFormat,
        Hints = exercise.Hints,
        HintModeActive = hintModeActive,
        IsAiGenerated = exercise.IsAIGenerated,
        CreatedAt = exercise.CreatedAt
    };
}
