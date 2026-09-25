using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Application.Features.Exercises;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Infrastructure.Persistence;
using Xunit;

namespace SwissTechTrainer.Application.Tests;

public class GetOrCreateCurrentExerciseQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ILLMClient _llmClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetOrCreateCurrentExerciseQueryHandler _sut;
    private readonly Guid _userId;

    public GetOrCreateCurrentExerciseQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=InMemoryTest_Ex_{Guid.NewGuid():N}.db")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var user = new UserProfile("TestCandidate", "test@swissdev.ch");
        _userId = user.Id;
        _context.UserProfiles.Add(user);
        _context.SaveChanges();

        _llmClient = Substitute.For<ILLMClient>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.GetOrCreateCurrentUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Username.Returns("TestCandidate");

        _sut = new GetOrCreateCurrentExerciseQueryHandler(_context, _llmClient, _currentUserService, new ExerciseLocalizationService());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WhenExistingUnpassedExerciseExists_ReusesSameExerciseWithoutCallingLlm()
    {
        // Arrange
        var existingExercise = new Exercise(
            category: CategoryType.DotNetDeepDive,
            level: DifficultyLevel.Level1,
            title: "Existing Deep Dive Exercise",
            description: "Desc",
            starterCode: "code"
        );
        _context.Exercises.Add(existingExercise);
        await _context.SaveChangesAsync();

        var query = new GetOrCreateCurrentExerciseQuery(CategoryType.DotNetDeepDive, ProgrammingLanguage.CSharp, _userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(existingExercise.Id);
        result.Title.Should().Be("Existing Deep Dive Exercise");
        await _llmClient.DidNotReceive().GenerateExerciseAsync(Arg.Any<ExerciseGenerationContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoExerciseExists_GeneratesNewViaLlmAndSaves()
    {
        // Arrange
        _llmClient.GenerateExerciseAsync(Arg.Any<ExerciseGenerationContext>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedExerciseDto
            {
                Title = "Newly Generated Exercise",
                Description = "Detailed description",
                StarterCode = "// starter",
                ExpectedOutputFormat = ".cs"
            });

        var query = new GetOrCreateCurrentExerciseQuery(CategoryType.Coding, ProgrammingLanguage.CSharp, _userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Newly Generated Exercise");
        result.IsAiGenerated.Should().BeTrue();

        var saved = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Newly Generated Exercise");
    }
}
