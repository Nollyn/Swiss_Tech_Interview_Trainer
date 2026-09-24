using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

public sealed record GenerateNewExerciseVariantCommand(CategoryType Category, Guid? UserId = null) : IRequest<ExerciseDto>;

public sealed class GenerateNewExerciseVariantCommandHandler : IRequestHandler<GenerateNewExerciseVariantCommand, ExerciseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILLMClient _llmClient;
    private readonly ICurrentUserService _currentUserService;

    public GenerateNewExerciseVariantCommandHandler(
        IApplicationDbContext context,
        ILLMClient llmClient,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _llmClient = llmClient;
        _currentUserService = currentUserService;
    }

    public async Task<ExerciseDto> Handle(GenerateNewExerciseVariantCommand request, CancellationToken cancellationToken)
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

        var latestExercise = await _context.Exercises
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
