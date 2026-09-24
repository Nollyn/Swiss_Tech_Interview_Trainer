namespace SwissTechTrainer.Application.Common.Interfaces;

/// <summary>
/// Defines a factory for creating instances of <see cref="IApplicationDbContext"/> to manage context lifecycles safely in asynchronous operations and long-lived circuits.
/// </summary>
public interface IApplicationDbContextFactory
{
    /// <summary>
    /// Creates a new <see cref="IApplicationDbContext"/> context instance.
    /// </summary>
    /// <returns>A new <see cref="IApplicationDbContext"/> instance.</returns>
    IApplicationDbContext CreateDbContext();

    /// <summary>
    /// Creates a new <see cref="IApplicationDbContext"/> context instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous creation operation, containing the new <see cref="IApplicationDbContext"/> instance.</returns>
    Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
