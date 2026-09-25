using FluentAssertions;
using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.Services;
using Xunit;

namespace SwissTechTrainer.Domain.Tests;

public class CategoryRubricCatalogTests
{
    [Theory]
    [InlineData(CategoryType.Coding)]
    [InlineData(CategoryType.SystemDesign)]
    [InlineData(CategoryType.LanguageDeepDive)]
    [InlineData(CategoryType.CleanCode)]
    [InlineData(CategoryType.ApiDesign)]
    [InlineData(CategoryType.Testing)]
    [InlineData(CategoryType.Behavioral)]
    public void GetRubricForCategory_AllCategoriesHaveValidRubricsWithSum100(CategoryType category)
    {
        var rubric = CategoryRubricCatalog.GetRubricForCategory(category);

        rubric.Should().NotBeNullOrEmpty();
        rubric.Count.Should().BeGreaterThanOrEqualTo(3);

        double totalWeight = rubric.Sum(r => r.Weight);
        totalWeight.Should().Be(100.0);

        foreach (var item in rubric)
        {
            item.Name.Should().NotBeNullOrWhiteSpace();
            item.Weight.Should().BeGreaterThan(0);
            item.Description.Should().NotBeNullOrWhiteSpace();
        }
    }
}
