using Microsoft.Extensions.Logging;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterNodeHandlerTests;

internal sealed class FilterNodeHandlerWithoutChildrenTests : TestBase
{
    [Test]
    public void HandleWithNoCondition_WithNullChildren_ReturnsEmptyString()
    {
        // Arrange
        FilterNode filterNode = new([]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(filterNode));
            result.ShouldBeEmpty();
        }
    }

    [Test]
    public void HandleWithNoCondition_WithNoChildren_ReturnsEmptyString()
    {
        // Arrange
        FilterNode filterNode = new(null, [], []);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(filterNode));
            result.ShouldBeEmpty();
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John")]
    public void HandleWithSingleCondition_WithNoChildren_ReturnsExpectedResult(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatorStr,
        string value
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterNode filterNode = new(
        [
            new(fieldName, filterOperator, value)
        ]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();

        string expectedConditionString = $"x.{fieldName} {expectedOperatorStr} \"{value}\"";
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)))
            .Returns(expectedConditionString);
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act
        string result = handler.Handle(filterNode);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            result.ShouldNotBeNullOrWhiteSpace();
            conditionHandler.Received(1).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            result.ShouldBe(expectedConditionString);
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.AndKey, "Age", FilterOperator.GtKey, ">", 30)]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.OrKey, "Age", FilterOperator.GtKey, ">", 30)]
    public void HandleWithMultipleConditions_WithNoChildren_ReturnsExpectedResult(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatiorStr,
        string value,
        string filterLogicStr,
        string fieldName2,
        string filterOperatorStr2,
        string expectedOperatiorStr2,
        int value2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterLogic filterLogic = filterLogicStr!;
        FilterNode filterNode = new(
            filterLogic,
            [
                new(fieldName, filterOperator, value),
                new(fieldName2, filterOperator2, value2),
            ]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();

        string expectedConditionString = $"x.{fieldName} {expectedOperatiorStr} \"{value}\"";
        string expectedConditionString2 = $"x.{fieldName2} {expectedOperatiorStr2} {value2}";
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)))
            .Returns(expectedConditionString);
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName2
                && c.Operator == filterOperator2
                && c.Value!.Equals(value2)))
            .Returns(expectedConditionString2);
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act
        string result = handler.Handle(filterNode);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            result.ShouldNotBeNullOrWhiteSpace();
            conditionHandler.Received(1).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            conditionHandler.Received(1).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName2
                && c.Operator == filterOperator2
                && c.Value!.Equals(value2)));
            result.ShouldBe($"{expectedConditionString} {filterLogic.Operator} {expectedConditionString2}");
        }
    }
}
