using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;
using SwissTechTrainer.Application.Features.Submissions;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;
using SwissTechTrainer.Infrastructure.Persistence;
using Xunit;

namespace SwissTechTrainer.Application.Tests;

public class SubmitExerciseCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ILLMClient _llmClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly SubmitExerciseCommandHandler _sut;
    private readonly Guid _userId;

    public SubmitExerciseCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=InMemoryTest_{Guid.NewGuid():N}.db")
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

        _sut = new SubmitExerciseCommandHandler(_context, _llmClient, _currentUserService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WhenEvaluationPassesThreshold_AdvancesLevelAndResetsFailures()
    {
        // Arrange
        var exercise = new Exercise(
            category: CategoryType.CleanCode,
            level: DifficultyLevel.Level1,
            title: "Clean Code Refactor",
            description: "Refactor legacy order processor",
            starterCode: "class OrderProcessor {}"
        );
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        var rubrics = CategoryRubricCatalog.GetRubricForCategory(CategoryType.CleanCode);
        var mockCriteria = rubrics.Select(r => new LlmEvaluationCriterionDto
        {
            Name = r.Name,
            Weight = r.Weight,
            Score = 95.0,
            Evidence = "Refactored cleanly into separate strategy classes",
            Rationale = "Strong SOLID adherence"
        }).ToList();

        _llmClient.EvaluateSubmissionAsync(Arg.Any<EvaluationPromptContext>(), Arg.Any<CancellationToken>())
            .Returns(new LlmEvaluationResponseDto
            {
                Criteria = mockCriteria,
                GeneralFeedback = "Outstanding senior submission.",
                ModelUsed = "MockModel",
                Suggestions = new List<LlmCodeSuggestionDto>()
            });

        var command = new SubmitExerciseCommand
        {
            ExerciseId = exercise.Id,
            SubmittedCode = "public class OrderProcessor { ... }",
            UserId = _userId
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DeterministicScore.Should().Be(95.0);
        result.PassedThreshold.Should().BeTrue();
        result.LevelAdvanced.Should().BeTrue();
        result.NewLevel.Should().Be(DifficultyLevel.Level2);
        result.ConsecutiveFailures.Should().Be(0);
        result.HintModeUnlocked.Should().BeFalse();

        // Check DB persisted state
        var savedSubmission = await _context.Submissions.Include(s => s.Evaluation).FirstOrDefaultAsync(s => s.ExerciseId == exercise.Id);
        savedSubmission.Should().NotBeNull();
        savedSubmission!.Evaluation.Should().NotBeNull();
        savedSubmission.Evaluation!.PassedThreshold.Should().BeTrue();
        savedSubmission.Evaluation.DeterministicScore.Should().Be(95.0);
    }

    [Fact]
    public async Task Handle_WhenFailingThreeConsecutiveTimes_UnlocksHintMode()
    {
        // Arrange
        var exercise = new Exercise(
            category: CategoryType.SystemDesign,
            level: DifficultyLevel.Level1,
            title: "Swiss Payment Ingestion",
            description: "Design high throughput service",
            starterCode: "# Design Doc"
        );
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        var rubrics = CategoryRubricCatalog.GetRubricForCategory(CategoryType.SystemDesign);
        var mockCriteria = rubrics.Select(r => new LlmEvaluationCriterionDto
        {
            Name = r.Name,
            Weight = r.Weight,
            Score = 75.0,
            Evidence = "Missing idempotency store",
            Rationale = "Need distributed deduplication"
        }).ToList();

        _llmClient.EvaluateSubmissionAsync(Arg.Any<EvaluationPromptContext>(), Arg.Any<CancellationToken>())
            .Returns(new LlmEvaluationResponseDto
            {
                Criteria = mockCriteria,
                GeneralFeedback = "Needs more detail on idempotency.",
                ModelUsed = "MockModel"
            });

        var command = new SubmitExerciseCommand
        {
            ExerciseId = exercise.Id,
            SubmittedCode = "Short design draft",
            UserId = _userId
        };

        // Act - 3 consecutive attempts
        var res1 = await _sut.Handle(command, CancellationToken.None);
        res1.ConsecutiveFailures.Should().Be(1);
        res1.HintModeUnlocked.Should().BeFalse();

        var res2 = await _sut.Handle(command, CancellationToken.None);
        res2.ConsecutiveFailures.Should().Be(2);
        res2.HintModeUnlocked.Should().BeFalse();

        var res3 = await _sut.Handle(command, CancellationToken.None);

        // Assert on 3rd attempt
        res3.ConsecutiveFailures.Should().Be(3);
        res3.HintModeUnlocked.Should().BeTrue();
        res3.PassedThreshold.Should().BeFalse();
        res3.LevelAdvanced.Should().BeFalse();
    }
}
