using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Submissions;

public sealed record GetCategoryHistoryQuery(CategoryType Category, Guid? UserId = null) : IRequest<List<CategoryAttemptHistoryItemDto>>;

public sealed class GetCategoryHistoryQueryHandler : IRequestHandler<GetCategoryHistoryQuery, List<CategoryAttemptHistoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoryHistoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<CategoryAttemptHistoryItemDto>> Handle(GetCategoryHistoryQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await _currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var submissions = await _context.Submissions
            .Include(s => s.Exercise)
            .Include(s => s.Evaluation)
            .Where(s => s.UserId == targetUserId && s.Exercise != null && s.Exercise.Category == request.Category)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        return submissions.Select(s => new CategoryAttemptHistoryItemDto
        {
            SubmissionId = s.Id,
            ExerciseId = s.ExerciseId,
            ExerciseTitle = s.Exercise?.Title ?? "Interview Exercise",
            Level = s.Exercise?.Level ?? DifficultyLevel.Level1,
            DeterministicScore = s.Evaluation?.DeterministicScore ?? 0.0,
            PassedThreshold = s.Evaluation?.PassedThreshold ?? false,
            SubmittedAt = s.SubmittedAt,
            GeneralFeedback = s.Evaluation?.GeneralFeedback ?? string.Empty,
            SubmittedCodePreview = s.SubmittedCode.Length > 120 ? s.SubmittedCode[..120] + "..." : s.SubmittedCode
        }).ToList();
    }
}
