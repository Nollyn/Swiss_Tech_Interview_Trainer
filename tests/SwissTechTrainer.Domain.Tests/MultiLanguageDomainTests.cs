using FluentAssertions;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;
using Xunit;

namespace SwissTechTrainer.Domain.Tests;

public class MultiLanguageDomainTests
{
    [Theory]
    [InlineData(ProgrammingLanguage.CSharp, ".cs", "csharp")]
    [InlineData(ProgrammingLanguage.Python, ".py", "python")]
    [InlineData(ProgrammingLanguage.Java, ".java", "java")]
    [InlineData(ProgrammingLanguage.Rust, ".rs", "rust")]
    [InlineData(ProgrammingLanguage.Go, ".go", "go")]
    [InlineData(ProgrammingLanguage.NodeJs, ".js", "javascript")]
    public void ProgrammingLanguageExtensions_ReturnsCorrectMetadata(
        ProgrammingLanguage language,
        string expectedExt,
        string expectedFence)
    {
        language.GetFileExtension().Should().Be(expectedExt);
        language.GetCodeFenceTag().Should().Be(expectedFence);
        language.GetDisplayName().Should().NotBeNullOrWhiteSpace();
        language.GetBadgeClass().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(ProgrammingLanguage.CSharp)]
    [InlineData(ProgrammingLanguage.Python)]
    [InlineData(ProgrammingLanguage.Java)]
    [InlineData(ProgrammingLanguage.Rust)]
    [InlineData(ProgrammingLanguage.Go)]
    [InlineData(ProgrammingLanguage.NodeJs)]
    public void CategoryRubrics_AllLanguagesAndCategories_WeightsSumTo100(ProgrammingLanguage language)
    {
        foreach (var category in Enum.GetValues<CategoryType>())
        {
            var rubric = CategoryRubricCatalog.GetRubricForCategory(category, language);

            rubric.Should().NotBeNull();
            rubric.Should().NotBeEmpty();

            double totalWeight = rubric.Sum(r => r.Weight);
            totalWeight.Should().BeApproximately(100.0, 0.001,
                $"Category '{category}' in '{language}' rubrics must sum to exactly 100%");
        }
    }

    [Fact]
    public void DeepDiveRubrics_AdaptsSpecificallyPerLanguage()
    {
        // Python: GIL, Asyncio, Decorators/Generators
        var pythonRubric = CategoryRubricCatalog.GetRubricForCategory(CategoryType.DotNetDeepDive, ProgrammingLanguage.Python);
        pythonRubric.Should().Contain(r => r.Name.Contains("GIL", StringComparison.OrdinalIgnoreCase));
        pythonRubric.Should().Contain(r => r.Name.Contains("Asyncio", StringComparison.OrdinalIgnoreCase));

        // Java: JVM, Modern GC, Concurrency
        var javaRubric = CategoryRubricCatalog.GetRubricForCategory(CategoryType.DotNetDeepDive, ProgrammingLanguage.Java);
        javaRubric.Should().Contain(r => r.Name.Contains("JVM", StringComparison.OrdinalIgnoreCase));
        javaRubric.Should().Contain(r => r.Name.Contains("Concurrency", StringComparison.OrdinalIgnoreCase));

        // Rust: Ownership, Borrow Checker, Lifetimes
        var rustRubric = CategoryRubricCatalog.GetRubricForCategory(CategoryType.DotNetDeepDive, ProgrammingLanguage.Rust);
        rustRubric.Should().Contain(r => r.Name.Contains("Borrow Checker", StringComparison.OrdinalIgnoreCase) || r.Name.Contains("Ownership", StringComparison.OrdinalIgnoreCase));

        // Go: Goroutines, Channels, CSP
        var goRubric = CategoryRubricCatalog.GetRubricForCategory(CategoryType.DotNetDeepDive, ProgrammingLanguage.Go);
        goRubric.Should().Contain(r => r.Name.Contains("Goroutines", StringComparison.OrdinalIgnoreCase));

        // Node.js: V8, Event Loop
        var nodeRubric = CategoryRubricCatalog.GetRubricForCategory(CategoryType.DotNetDeepDive, ProgrammingLanguage.NodeJs);
        nodeRubric.Should().Contain(r => r.Name.Contains("V8", StringComparison.OrdinalIgnoreCase));
        nodeRubric.Should().Contain(r => r.Name.Contains("Event Loop", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UserProfile_LanguageProgressionIsIsolated()
    {
        var user = UserProfile.Create("ZurichPolyglot", "polyglot@zurich.ch");

        var csharpCoding = user.GetOrCreateProgress(CategoryType.Coding, ProgrammingLanguage.CSharp);
        var pythonCoding = user.GetOrCreateProgress(CategoryType.Coding, ProgrammingLanguage.Python);
        var rustCoding = user.GetOrCreateProgress(CategoryType.Coding, ProgrammingLanguage.Rust);

        // Advance Python only
        pythonCoding.RecordAttempt(95.0, true);
        pythonCoding.RecordAttempt(94.0, true);

        // Assert Python progressed to Level 3
        pythonCoding.CurrentLevel.Should().Be(DifficultyLevel.Level3);
        pythonCoding.CompletedLevelsCount.Should().Be(2);

        // Assert C# and Rust remained at Level 1 (fully isolated tracks)
        csharpCoding.CurrentLevel.Should().Be(DifficultyLevel.Level1);
        csharpCoding.CompletedLevelsCount.Should().Be(0);

        rustCoding.CurrentLevel.Should().Be(DifficultyLevel.Level1);
        rustCoding.CompletedLevelsCount.Should().Be(0);
    }
}
