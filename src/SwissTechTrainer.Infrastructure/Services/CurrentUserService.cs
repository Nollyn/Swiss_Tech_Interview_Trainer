using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Infrastructure.Persistence;

namespace SwissTechTrainer.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly AppDbContext _context;
    private Guid? _cachedUserId;

    public Guid UserId => _cachedUserId ?? Guid.Empty;
    public string Username => "SwissTechLead_Candidate";

    public CurrentUserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> GetOrCreateCurrentUserIdAsync(CancellationToken ct = default)
    {
        if (_cachedUserId.HasValue && _cachedUserId.Value != Guid.Empty)
        {
            return _cachedUserId.Value;
        }

        var user = await _context.UserProfiles.FirstOrDefaultAsync(ct);
        if (user == null)
        {
            user = new UserProfile(Username, "candidate.zurich@swissdev.ch", "Senior .NET Developer / Tech Lead (Zurich)");
            _context.UserProfiles.Add(user);
            await _context.SaveChangesAsync(ct);
        }

        _cachedUserId = user.Id;
        return user.Id;
    }
}
