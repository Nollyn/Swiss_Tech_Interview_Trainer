using MediatR;
using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Submissions;

/// <summary>
/// Query to retrieve chronological historical attempts and evaluation results for a specific interview category and optional language filter.
/// </summary>
/// <param name="Category">The target category dimension.</param>
/// <param name="Language">Optional programming language filter.</param>
/// <param name="UserId">Optional user ID filter, defaulting to current ambient user.</param>
public sealed record GetCategoryHistoryQuery(CategoryType Category, ProgrammingLanguage? Language = null, Guid? UserId = null) : IRequest<List<CategoryAttemptHistoryItemDto>>;

/// <summary>
/// Handler that loads past submissions and evaluations for auditability and candidate progression review.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class GetCategoryHistoryQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetCategoryHistoryQuery, List<CategoryAttemptHistoryItemDto>>
{
    /// <summary>
    /// Executes the query to load chronological historical attempts.
    /// </summary>
    /// <param name="request">The category history query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of past submission attempts with evaluation outcomes.</returns>
    public async Task<List<CategoryAttemptHistoryItemDto>> Handle(GetCategoryHistoryQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? await currentUserService.GetOrCreateCurrentUserIdAsync(cancellationToken);

        var query = context.Submissions
            .Include(s => s.Exercise)
            .Include(s => s.Evaluation)
            .Where(s => s.UserId == targetUserId && s.Exercise != null && s.Exercise.Category == request.Category);

        if (request.Language.HasValue)
        {
            query = query.Where(s => s.Language == request.Language.Value || (s.Exercise != null && s.Exercise.Language == request.Language.Value));
        }

        var submissions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        return
        [
            .. submissions.Select(s => new CategoryAttemptHistoryItemDto
            {
                SubmissionId = s.Id,
                ExerciseId = s.ExerciseId,
                ExerciseTitle = s.Exercise?.Title ?? "Interview Exercise",
                Language = s.Language,
                Level = s.Exercise?.Level ?? DifficultyLevel.Level1,
                DeterministicScore = s.Evaluation?.DeterministicScore ?? 0.0,
                PassedThreshold = s.Evaluation?.PassedThreshold ?? false,
                SubmittedAt = s.SubmittedAt,
                GeneralFeedback = s.Evaluation?.GeneralFeedback ?? string.Empty,
                SubmittedCodePreview = s.SubmittedCode.Length > 120 ? s.SubmittedCode[..120] + "..." : s.SubmittedCode
            })
        ];
    }
}
