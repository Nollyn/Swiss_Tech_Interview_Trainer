using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwissTechTrainer.Application;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Features.Dashboard;
using SwissTechTrainer.Application.Features.Exercises;
using SwissTechTrainer.Application.Features.Submissions;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Infrastructure.LLM;
using SwissTechTrainer.Infrastructure.Persistence;
using SwissTechTrainer.Infrastructure.Services;
using Xunit;

namespace SwissTechTrainer.IntegrationTests;

public class EvaluationPipelineIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly string _dbName = $"IntegrationDb_{Guid.NewGuid():N}.db";

    public EvaluationPipelineIntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite($"Data Source={_dbName}"));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICodeFileParser, CodeFileParser>();
        services.AddSingleton<DeterministicMockLlmClient>();
        services.AddScoped<ILLMClient>(sp => sp.GetRequiredService<DeterministicMockLlmClient>());

        services.AddApplication();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<AppDbContext>();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task FullPipeline_CandidateProgression_CompletesSuccessfullyEndToEnd()
    {
        // 1. Candidate views dashboard
        var initialDashboard = await _mediator.Send(new GetUserDashboardQuery());
        initialDashboard.Should().NotBeNull();
        initialDashboard.Categories.Should().HaveCount(7);
        var cleanCodeCat = initialDashboard.Categories.First(c => c.Category == CategoryType.CleanCode);
        cleanCodeCat.CurrentLevel.Should().Be(DifficultyLevel.Level1);
        cleanCodeCat.CompletedLevelsCount.Should().Be(0);

        // 2. Candidate retrieves current exercise for Clean Code
        var exerciseDto = await _mediator.Send(new GetOrCreateCurrentExerciseQuery(CategoryType.CleanCode));
        exerciseDto.Should().NotBeNull();
        exerciseDto.Level.Should().Be(DifficultyLevel.Level1);
        exerciseDto.Title.Should().NotBeNullOrWhiteSpace();

        // 3. Candidate submits code solution
        string candidateCode = """
using System;

namespace SwissWealth.Refactored;

public interface IFeeStrategy
{
    decimal Calculate(decimal portfolioValue, int tradeCount);
}

public class PrivateClientFeeStrategy : IFeeStrategy
{
    public decimal Calculate(decimal portfolioValue, int tradeCount)
    {
        if (portfolioValue < 0) throw new ArgumentOutOfRangeException(nameof(portfolioValue));
        decimal baseRate = portfolioValue > 1_000_000m ? 0.002m : 0.005m;
        return (portfolioValue * baseRate) + (tradeCount * 12.5m);
    }
}
""";

        var submissionResult = await _mediator.Send(new SubmitExerciseCommand
        {
            ExerciseId = exerciseDto.Id,
            SubmittedCode = candidateCode,
            AdditionalNotes = "Applied Strategy Pattern and explicit bounds validation."
        });

        // 4. Verify evaluation results
        submissionResult.Should().NotBeNull();
        submissionResult.DeterministicScore.Should().BeGreaterThanOrEqualTo(90.0);
        submissionResult.PassedThreshold.Should().BeTrue();
        submissionResult.LevelAdvanced.Should().BeTrue();
        submissionResult.NewLevel.Should().Be(DifficultyLevel.Level2);
        submissionResult.CriteriaBreakdown.Should().NotBeEmpty();
        submissionResult.CodeSuggestions.Should().NotBeEmpty();

        // 5. Candidate queries category history
        var history = await _mediator.Send(new GetCategoryHistoryQuery(CategoryType.CleanCode));
        history.Should().HaveCount(1);
        history[0].PassedThreshold.Should().BeTrue();
        history[0].DeterministicScore.Should().Be(submissionResult.DeterministicScore);

        // 6. Updated dashboard reflects level 2 unlocked
        var updatedDashboard = await _mediator.Send(new GetUserDashboardQuery());
        var updatedCleanCode = updatedDashboard.Categories.First(c => c.Category == CategoryType.CleanCode);
        updatedCleanCode.CurrentLevel.Should().Be(DifficultyLevel.Level2);
        updatedCleanCode.CompletedLevelsCount.Should().Be(1);
        updatedCleanCode.ProgressPercentage.Should().Be(20.0); // 1/5
    }
}
