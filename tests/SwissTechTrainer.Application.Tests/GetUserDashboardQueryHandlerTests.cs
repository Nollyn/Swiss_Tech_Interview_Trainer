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
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetUserDashboardQueryHandler _sut;
    private readonly Guid _userId;

    public GetUserDashboardQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=InMemoryTest_Dash_{Guid.NewGuid():N}.db")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var user = new UserProfile("SwissProCandidate", "candidate@swiss.ch");
        _userId = user.Id;
        _context.UserProfiles.Add(user);
        _context.SaveChanges();

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.GetOrCreateCurrentUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Username.Returns("SwissProCandidate");

        _sut = new GetUserDashboardQueryHandler(_context, _currentUserService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ReturnsAllSevenCategoriesWithCalculatedProgress()
    {
        // Arrange: record progress in Clean Code (pass 2 levels)
        var user = await _context.UserProfiles.Include(u => u.Progresses).FirstAsync(u => u.Id == _userId);
        var cleanCodeProg = user.GetOrCreateProgress(CategoryType.CleanCode);
        cleanCodeProg.RecordAttempt(95.0, true);
        cleanCodeProg.RecordAttempt(92.0, true);
        await _context.SaveChangesAsync();

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
