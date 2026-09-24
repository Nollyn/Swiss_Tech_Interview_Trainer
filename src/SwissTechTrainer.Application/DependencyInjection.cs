using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwissTechTrainer.Application.Common.Behaviors;

namespace SwissTechTrainer.Application;

/// <summary>
/// Provides extension methods for registering Application layer services and MediatR pipeline behaviors in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds MediatR handlers, pipeline behaviors (Validation, Logging, Performance), and FluentValidation validators to the service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddLogging();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
