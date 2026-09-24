using FluentValidation;
using MediatR;

namespace SwissTechTrainer.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that runs all registered FluentValidation validators for the incoming MediatR request before proceeding to the handler.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="validators">The collection of registered validators for <typeparamref name="TRequest"/>.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Validates the request against all registered rules and proceeds if valid; otherwise throws a <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="request">The incoming request object.</param>
    /// <param name="next">The next delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response produced by the next handler in the pipeline.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation failures occur.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
