using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SwissTechTrainer.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that tracks execution duration of MediatR requests and emits performance warnings when thresholds are exceeded.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="logger">The structured logger instance.</param>
public sealed class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long WarningThresholdMs = 2000;

    /// <summary>
    /// Measures handler execution duration and logs warning telemetry if execution exceeds warning thresholds.
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
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        var elapsedMs = timer.ElapsedMilliseconds;

        if (elapsedMs > WarningThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogWarning(
                "Long running MediatR request detected: {RequestName} ({ElapsedMilliseconds} ms)",
                requestName,
                elapsedMs);
        }

        return response;
    }
}
