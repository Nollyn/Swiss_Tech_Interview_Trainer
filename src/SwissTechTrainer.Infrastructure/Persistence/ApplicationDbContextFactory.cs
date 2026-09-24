using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Application.Common.Interfaces;

namespace SwissTechTrainer.Infrastructure.Persistence;

/// <summary>
/// Infrastructure implementation of <see cref="IApplicationDbContextFactory"/> that delegates to EF Core's <see cref="IDbContextFactory{TContext}"/>.
/// </summary>
/// <param name="dbContextFactory">The underlying EF Core DbContext factory.</param>
public class ApplicationDbContextFactory(IDbContextFactory<AppDbContext> dbContextFactory) : IApplicationDbContextFactory
{
    /// <inheritdoc />
    public IApplicationDbContext CreateDbContext()
    {
        return dbContextFactory.CreateDbContext();
    }

    /// <inheritdoc />
    public async Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await dbContextFactory.CreateDbContextAsync(cancellationToken);
    }
}
