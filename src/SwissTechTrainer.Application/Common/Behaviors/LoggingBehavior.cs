using MediatR;
using Microsoft.Extensions.Logging;

namespace SwissTechTrainer.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that logs structured diagnostics regarding incoming requests and completed responses across MediatR handlers.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="logger">The structured logger instance.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Logs request processing lifecycle events and delegates execution to the next pipeline step.
    /// </summary>
    /// <param name="request">The incoming request object.</param>
    /// <param name="next">The next delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response produced by the next handler in the pipeline.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing MediatR request: {RequestName}", requestName);

        var response = await next();

        logger.LogInformation("Successfully completed MediatR request: {RequestName}", requestName);

        return response;
    }
}
