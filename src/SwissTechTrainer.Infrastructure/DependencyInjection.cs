using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Infrastructure.LLM;
using SwissTechTrainer.Infrastructure.Persistence;
using SwissTechTrainer.Infrastructure.Services;

namespace SwissTechTrainer.Infrastructure;

/// <summary>
/// Provides extension methods for registering Infrastructure dependencies (EF Core, LLM Provider strategies, file parsers, and user services).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers database contexts, HTTP clients with Polly resilience, and interchangeable LLM strategies.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Configuration (Postgres default with SQLite fallback for offline local runs)
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        string dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";

        if (string.Equals(dbProvider, "Postgres", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, b =>
                {
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));
        }
        else
        {
            // Default portable Sqlite for instant zero-dependency execution
            string sqlitePath = connectionString ?? "Data Source=SwissTechTrainer.db";
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(sqlitePath, b =>
                {
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // 2. Application Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICodeFileParser, CodeFileParser>();

        // 3. LLM Strategy & Clients
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));

        services.AddHttpClient<GroqLlmClient>();
        services.AddHttpClient<OllamaLlmClient>();
        services.AddSingleton<DeterministicMockLlmClient>();

        services.AddScoped<ILLMClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LlmOptions>>().Value;
            var provider = options.Provider?.Trim();

            if (string.Equals(provider, "Groq", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<GroqLlmClient>();
            }

            if (string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<OllamaLlmClient>();
            }

            return sp.GetRequiredService<DeterministicMockLlmClient>();
        });

        return services;
    }
}
