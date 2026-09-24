using System.Globalization;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SwissTechTrainer.Application;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Features.Dashboard;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Infrastructure.LLM;
using SwissTechTrainer.Infrastructure.Persistence;
using SwissTechTrainer.Infrastructure.Services;
using SwissTechTrainer.Web.Resources;
using Xunit;

namespace SwissTechTrainer.IntegrationTests;

public class LocalizationAndQueryOptimizationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly string _dbName = $"QueryOptDb_{Guid.NewGuid():N}.db";

    public LocalizationAndQueryOptimizationTests()
    {
        var services = new ServiceCollection();

        services.AddLocalization();

        services.AddDbContextFactory<AppDbContext>(opts =>
        {
            opts.UseSqlite($"Data Source={_dbName}", b =>
            {
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

        services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlite($"Data Source={_dbName}", b =>
            {
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

        services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICodeFileParser, CodeFileParser>();
        services.AddSingleton<DeterministicMockLlmClient>();
        services.AddScoped<ILLMClient>(sp => sp.GetRequiredService<DeterministicMockLlmClient>());

        services.AddApplication();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<AppDbContext>();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _localizer = _serviceProvider.GetRequiredService<IStringLocalizer<SharedResources>>();
    }

    public async Task InitializeAsync()
    {
        await DatabaseInitializer.InitializeAsync(_context);
        await DatabaseSeeder.SeedAsync(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Theory]
    [InlineData("en", "AppTitle", "Swiss Tech Interview Trainer")]
    [InlineData("de", "AppTitle", "Schweizer Tech-Interview Trainer")]
    [InlineData("es", "AppTitle", "Entrenador de Entrevistas Tech Suiza")]
    [InlineData("en", "HiringTrackBadge", "Swiss Tech Hiring Track")]
    [InlineData("de", "HiringTrackBadge", "Schweizer Tech-Rekrutierungstrack")]
    [InlineData("es", "HiringTrackBadge", "Track de Contratación Tech Suiza")]
    [InlineData("en", "TargetRoleLabel", "Target Role:")]
    [InlineData("de", "TargetRoleLabel", "Zielrolle:")]
    [InlineData("es", "TargetRoleLabel", "Rol Objetivo:")]
    [InlineData("en", "InterviewCategoriesTitle", "Interview Categories")]
    [InlineData("de", "InterviewCategoriesTitle", "Interview-Kategorien")]
    [InlineData("es", "InterviewCategoriesTitle", "Categorías de Entrevista")]
    [InlineData("en", "PracticeLevelButton", "Practice Level {0}")]
    [InlineData("de", "PracticeLevelButton", "Stufe {0} üben")]
    [InlineData("es", "PracticeLevelButton", "Practicar Nivel {0}")]
    [InlineData("en", "CategoryDesc_Coding", "Pragmatic algorithms")]
    [InlineData("es", "CategoryDesc_Coding", "Algoritmos pragmáticos")]
    [InlineData("de", "CategoryDesc_Coding", "Pragmatische Algorithmen")]
    [InlineData("en", "CategoryDesc_SystemDesign", "Distributed service design")]
    [InlineData("es", "CategoryDesc_SystemDesign", "Diseño de servicios distribuidos")]
    [InlineData("de", "CategoryDesc_SystemDesign", "Verteiltes Servicedesign")]
    [InlineData("es", "CategoryDesc_DotNetDeepDive_CSharp", "Gestión de memoria")]
    [InlineData("es", "CategoryDesc_CleanCode", "Refactorización de code smells")]
    [InlineData("es", "CategoryDesc_ApiDesign", "Contratos REST/gRPC")]
    [InlineData("es", "CategoryDesc_Testing", "Pruebas unitarias")]
    [InlineData("es", "CategoryDesc_Behavioral", "Liderazgo técnico")]
    public void StringLocalizer_ResolvesKeysCorrectly_ForSupportedCultures(string cultureName, string key, string expectedSubstring)
    {
        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var localizedString = _localizer[key];

            localizedString.ResourceNotFound.Should().BeFalse($"Key '{key}' should be found in culture '{cultureName}'");
            localizedString.Value.Should().Contain(expectedSubstring);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task GetUserDashboardQuery_WithSplitQueriesAndDeterministicOrdering_ExecutesWithoutCartesianExplosion()
    {
        // Add additional progresses and submissions to create multi-collection load
        var user = await _context.UserProfiles.FirstAsync();
        var exercise = await _context.Exercises.FirstAsync();

        var sub1 = Submission.Create(exercise.Id, user.Id, "code 1", language: ProgrammingLanguage.CSharp);
        var sub2 = Submission.Create(exercise.Id, user.Id, "code 2", language: ProgrammingLanguage.Python);
        _context.Submissions.AddRange(sub1, sub2);
        await _context.SaveChangesAsync();

        // Act
        var dashboardCSharp = await _mediator.Send(new GetUserDashboardQuery(ProgrammingLanguage.CSharp, user.Id));
        var dashboardPython = await _mediator.Send(new GetUserDashboardQuery(ProgrammingLanguage.Python, user.Id));

        // Assert
        dashboardCSharp.Should().NotBeNull();
        dashboardCSharp.Categories.Should().HaveCount(7);
        dashboardCSharp.SelectedLanguage.Should().Be(ProgrammingLanguage.CSharp);

        dashboardPython.Should().NotBeNull();
        dashboardPython.Categories.Should().HaveCount(7);
        dashboardPython.SelectedLanguage.Should().Be(ProgrammingLanguage.Python);

        // Verify dynamic adaptation of Category Titles & Descriptions per language track
        var codingCSharp = dashboardCSharp.Categories.First(c => c.Category == CategoryType.Coding);
        var codingPython = dashboardPython.Categories.First(c => c.Category == CategoryType.Coding);
        codingCSharp.DisplayName.Should().Be("Coding / Algorithms (C# (.NET))");
        codingPython.DisplayName.Should().Be("Coding / Algorithms (Python)");

        var deepDiveCSharp = dashboardCSharp.Categories.First(c => c.Category == CategoryType.DotNetDeepDive);
        var deepDivePython = dashboardPython.Categories.First(c => c.Category == CategoryType.DotNetDeepDive);
        deepDiveCSharp.DisplayName.Should().Be(".NET / C# Deep Dive");
        deepDivePython.DisplayName.Should().Be("Python Deep Dive");
        deepDivePython.ShortDescription.Should().Contain("GIL");
    }

    [Theory]
    [InlineData("http://localhost:8080/exercise/Coding?lang=Python", "/exercise/Coding?lang=Python")]
    [InlineData("https://app.swisstech.ch/dashboard", "/dashboard")]
    [InlineData("/history?lang=Python", "/history?lang=Python")]
    [InlineData("history", "/history")]
    [InlineData("", "/")]
    [InlineData("http://evil.com/attack", "/attack")]
    [InlineData("//evil.com/exploit", "/")]
    [InlineData("/\\evil.com/exploit", "/")]
    public async Task CultureEndpoint_SanitizesRedirectUri_AndRedirectsLocallyWithoutExceptions(string redirectUri, string expectedTarget)
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var encodedUri = Uri.EscapeDataString(redirectUri);
        var response = await client.GetAsync($"/api/culture/set?culture=de&redirectUri={encodedUri}");

        // Assert
        ((int)response.StatusCode).Should().BeInRange(300, 399);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.OriginalString.Should().Be(expectedTarget);
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookieHeader = string.Join(";", response.Headers.GetValues("Set-Cookie"));
        cookieHeader.Should().Contain(".AspNetCore.Culture");
    }
}
