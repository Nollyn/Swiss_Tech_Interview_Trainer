namespace SwissTechTrainer.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Username { get; }
    Task<Guid> GetOrCreateCurrentUserIdAsync(CancellationToken ct = default);
}
