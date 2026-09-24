using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Features.Dashboard;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Infrastructure.Persistence;
using Xunit;

namespace SwissTechTrainer.Application.Tests;

public class GetUserDashboardQueryHandlerTests : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContextFactory _contextFactory;
    private readonly GetUserDashboardQueryHandler _sut;
    private readonly Guid _userId;

    public GetUserDashboardQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=InMemoryTest_Dash_{Guid.NewGuid():N}.db")
            .Options;

        using (var setupContext = new AppDbContext(_options))
        {
            setupContext.Database.EnsureCreated();

            var user = new UserProfile("SwissProCandidate", "candidate@swiss.ch");
            _userId = user.Id;
            setupContext.UserProfiles.Add(user);
            setupContext.SaveChanges();
        }

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.GetOrCreateCurrentUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Username.Returns("SwissProCandidate");

        _contextFactory = Substitute.For<IApplicationDbContextFactory>();
        _contextFactory.CreateDbContext().Returns(_ => new AppDbContext(_options));
        _contextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult<IApplicationDbContext>(new AppDbContext(_options)));

        _sut = new GetUserDashboardQueryHandler(_contextFactory, _currentUserService);
    }

    public void Dispose()
    {
        using var cleanupContext = new AppDbContext(_options);
        cleanupContext.Database.EnsureDeleted();
    }

    [Fact]
    public async Task Handle_ReturnsAllSevenCategoriesWithCalculatedProgress()
    {
        // Arrange: record progress in Clean Code (pass 2 levels)
        using (var arrangeContext = new AppDbContext(_options))
        {
            var user = await arrangeContext.UserProfiles.Include(u => u.Progresses).FirstAsync(u => u.Id == _userId);
            var cleanCodeProg = user.GetOrCreateProgress(CategoryType.CleanCode);
            cleanCodeProg.RecordAttempt(95.0, true);
            cleanCodeProg.RecordAttempt(92.0, true);
            await arrangeContext.SaveChangesAsync();
        }

        var query = new GetUserDashboardQuery(ProgrammingLanguage.CSharp, _userId);

        // Act
        var dashboard = await _sut.Handle(query, CancellationToken.None);

        // Assert
        dashboard.Should().NotBeNull();
        dashboard.Categories.Should().HaveCount(7);

        var cleanCodeDto = dashboard.Categories.First(c => c.Category == CategoryType.CleanCode);
        cleanCodeDto.CompletedLevelsCount.Should().Be(2);
        cleanCodeDto.CurrentLevel.Should().Be(DifficultyLevel.Level3);
        cleanCodeDto.ProgressPercentage.Should().Be(40.0); // 2/5 * 100%
        cleanCodeDto.HighestScoreAchieved.Should().Be(95.0);

        dashboard.TotalCompletedExercises.Should().Be(2);
        dashboard.OverallMasteryPercentage.Should().BeGreaterThan(0);
    }
}
