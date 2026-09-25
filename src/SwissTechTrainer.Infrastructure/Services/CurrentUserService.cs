using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Infrastructure.Persistence;

namespace SwissTechTrainer.Infrastructure.Services;

/// <summary>
/// Service resolving or persisting the ambient single-user candidate session for the application.
/// </summary>
/// <param name="context">The database context.</param>
public class CurrentUserService(AppDbContext context) : ICurrentUserService
{
    private Guid? _cachedUserId;

    /// <inheritdoc />
    public Guid UserId => _cachedUserId ?? Guid.Empty;

    /// <inheritdoc />
    public string Username => "Swiss Tech Lead Candidate";

    /// <inheritdoc />
    public async Task<Guid> GetOrCreateCurrentUserIdAsync(CancellationToken ct = default)
    {
        if (_cachedUserId.HasValue && _cachedUserId.Value != Guid.Empty)
        {
            return _cachedUserId.Value;
        }

        var user = await context.UserProfiles.FirstOrDefaultAsync(ct);
        if (user == null)
        {
            user = UserProfile.Create(Username, "candidate.zurich@swissdev.ch", "Senior Developer / Tech Lead (Zurich)");
            context.UserProfiles.Add(user);
            await context.SaveChangesAsync(ct);
        }

        _cachedUserId = user.Id;
        return user.Id;
    }
}
