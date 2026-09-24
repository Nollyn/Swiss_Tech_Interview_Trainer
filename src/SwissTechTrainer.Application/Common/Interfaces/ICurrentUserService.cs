namespace SwissTechTrainer.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated or ambient session user context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user ID if already resolved in ambient context.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets the current candidate username.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Resolves the existing candidate profile ID or creates a default persistent profile if none exists.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved candidate user ID.</returns>
    Task<Guid> GetOrCreateCurrentUserIdAsync(CancellationToken ct = default);
}
