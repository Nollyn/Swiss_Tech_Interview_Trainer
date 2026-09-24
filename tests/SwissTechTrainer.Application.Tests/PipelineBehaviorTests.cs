using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SwissTechTrainer.Application.Common.Behaviors;
using Xunit;

namespace SwissTechTrainer.Application.Tests;

public sealed record SampleTestCommand(string Name) : IRequest<string>;

public class SampleTestCommandValidator : AbstractValidator<SampleTestCommand>
{
    public SampleTestCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class PipelineBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_Should_ThrowValidationException_When_ValidationFails()
    {
        // Arrange
        var validator = new SampleTestCommandValidator();
        var behavior = new ValidationBehavior<SampleTestCommand, string>([validator]);
        var command = new SampleTestCommand("");
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        Func<Task> act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name is required.*");
    }

    [Fact]
    public async Task ValidationBehavior_Should_ProceedToNext_When_ValidationSucceeds()
    {
        // Arrange
        var validator = new SampleTestCommandValidator();
        var behavior = new ValidationBehavior<SampleTestCommand, string>([validator]);
        var command = new SampleTestCommand("ValidName");
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task LoggingBehavior_Should_CallNextDelegateAndLog()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<SampleTestCommand, string>>>();
        var behavior = new LoggingBehavior<SampleTestCommand, string>(logger);
        var command = new SampleTestCommand("Test");
        RequestHandlerDelegate<string> next = () => Task.FromResult("LoggedSuccess");

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be("LoggedSuccess");
    }

    [Fact]
    public async Task PerformanceBehavior_Should_CallNextDelegate()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PerformanceBehavior<SampleTestCommand, string>>>();
        var behavior = new PerformanceBehavior<SampleTestCommand, string>(logger);
        var command = new SampleTestCommand("Test");
        RequestHandlerDelegate<string> next = () => Task.FromResult("PerfSuccess");

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be("PerfSuccess");
    }
}
