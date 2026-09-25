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
using SwissTechTrainer.Application.Features.Exercises;
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
    [InlineData("es", "CategoryDesc_LanguageDeepDive_CSharp", "Gestión de memoria")]
    [InlineData("es", "CategoryDesc_CleanCode", "Refactorización de code smells")]
    [InlineData("es", "CategoryDesc_ApiDesign", "Contratos REST/gRPC")]
    [InlineData("es", "CategoryDesc_Testing", "Pruebas unitarias")]
    [InlineData("es", "CategoryDesc_Behavioral", "Liderazgo técnico")]
    [InlineData("en", "ThemeLight", "Light")]
    [InlineData("es", "ThemeLight", "Claro")]
    [InlineData("de", "ThemeLight", "Hell")]
    [InlineData("en", "ThemeDark", "Dark")]
    [InlineData("es", "ThemeDark", "Oscuro")]
    [InlineData("de", "ThemeDark", "Dunkel")]
    [InlineData("es", "CandidateRoleBadge", "Candidato Tech Lead Suizo")]
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

        var deepDiveCSharp = dashboardCSharp.Categories.First(c => c.Category == CategoryType.LanguageDeepDive);
        var deepDivePython = dashboardPython.Categories.First(c => c.Category == CategoryType.LanguageDeepDive);
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

    [Theory]
    [InlineData("light", "light")]
    [InlineData("dark", "dark")]
    [InlineData("unknown", "dark")]
    public async Task ThemeEndpoint_SetsAppThemeCookie_AndRedirectsLocally(string themeInput, string expectedThemeValue)
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync($"/api/theme/set?theme={themeInput}&redirectUri=%2Fdashboard");

        // Assert
        ((int)response.StatusCode).Should().BeInRange(300, 399);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.OriginalString.Should().Be("/dashboard");
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookieHeader = string.Join(";", response.Headers.GetValues("Set-Cookie"));
        cookieHeader.Should().Contain($"app_theme={expectedThemeValue}");
    }

    [Fact]
    public async Task CurrentUser_And_DatabaseSeeder_ProvideCleanFormattedUsername()
    {
        var currentUserService = _serviceProvider.GetRequiredService<ICurrentUserService>();
        currentUserService.Username.Should().Be("Swiss Tech Lead Candidate");

        var user = await _context.UserProfiles.FirstAsync();
        user.Username.Should().Be("Swiss Tech Lead Candidate");
        user.Username.Should().NotContain("_");
    }

    [Theory]
    [InlineData("es", CategoryType.Coding, "Bolsa Financiera de Zúrich — Rastreador de Tasa y Volumen del Flujo de Órdenes", "Especificación del Problema", "Restricciones")]
    [InlineData("de", CategoryType.Coding, "Zürcher Finanzbörse — Auftragsfluss-Raten- & Volumen-Tracker", "Problemspezifikation", "Einschränkungen")]
    [InlineData("en", CategoryType.Coding, "Zurich Financial Exchange — Order Stream Rate & Volume Tracker", "Problem Specification", "Constraints")]
    [InlineData("es", CategoryType.SystemDesign, "Servicio Suizo de Notificación de Pagos Interbancarios", "Requisitos", "Entregable")]
    [InlineData("de", CategoryType.SystemDesign, "Schweizer Interbank-Zahlungsbenachrichtigungsdienst", "Anforderungen", "Ergebnis")]
    [InlineData("es", CategoryType.LanguageDeepDive, "Optimización de Memoria CLR y Span<T>", "Requisitos", "")]
    [InlineData("es", CategoryType.CleanCode, "Refactorización del Motor de Tarifas de Gestión Patrimonial", "Tareas", "")]
    [InlineData("es", CategoryType.ApiDesign, "Controlador de API de Transferencias SEPA / SIC Suizas Idempotente", "Requisitos", "")]
    [InlineData("es", CategoryType.Testing, "Validación de Checksum de IBAN Suizo (CH / LI) mediante TDD", "Requisitos", "")]
    [InlineData("es", CategoryType.Behavioral, "Escenario de Tech Lead Empresarial Suizo: Modernización vs Plazos de Entrega", "Escenario", "Tarea")]
    public void ExerciseLocalizationService_LocalizesChallengeContent_AcrossSupportedCultures(
        string cultureCode,
        CategoryType category,
        string expectedTitleSubtext,
        string expectedHeading1,
        string expectedHeading2)
    {
        var localizer = _serviceProvider.GetRequiredService<IExerciseLocalizationService>();

        var originalExercise = _context.Exercises.First(e => e.Category == category && e.Level == DifficultyLevel.Level1);

        var localizedTitle = localizer.LocalizeTitle(category, DifficultyLevel.Level1, originalExercise.Title, cultureCode);
        var localizedDesc = localizer.LocalizeDescription(category, DifficultyLevel.Level1, originalExercise.Description, cultureCode);
        var localizedHints = localizer.LocalizeHints(category, DifficultyLevel.Level1, originalExercise.Hints, cultureCode);

        localizedTitle.Should().NotBeNullOrWhiteSpace();
        localizedDesc.Should().Contain(expectedTitleSubtext);
        if (!string.IsNullOrEmpty(expectedHeading1))
        {
            localizedDesc.Should().Contain(expectedHeading1);
        }
        if (!string.IsNullOrEmpty(expectedHeading2))
        {
            localizedDesc.Should().Contain(expectedHeading2);
        }
        localizedHints.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(ProgrammingLanguage.CSharp, "bg-primary text-white", "C# (.NET)")]
    [InlineData(ProgrammingLanguage.Python, "bg-warning text-dark", "Python")]
    [InlineData(ProgrammingLanguage.Java, "bg-danger text-white", "Java")]
    [InlineData(ProgrammingLanguage.Rust, "bg-secondary text-white", "Rust")]
    [InlineData(ProgrammingLanguage.Go, "bg-info text-dark", "Go")]
    [InlineData(ProgrammingLanguage.NodeJs, "bg-success text-white", "Node.js")]
    public void ProgrammingLanguageExtensions_ProvidesValidBootstrapBadgeClasses(
        ProgrammingLanguage language,
        string expectedBadgeClass,
        string expectedDisplayName)
    {
        language.GetBadgeClass().Should().Be(expectedBadgeClass);
        language.GetDisplayName().Should().Be(expectedDisplayName);
    }
}
