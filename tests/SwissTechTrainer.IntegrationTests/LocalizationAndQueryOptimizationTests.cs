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

        services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlite($"Data Source={_dbName}", b =>
            {
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

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
    }
}
