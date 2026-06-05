using Microsoft.Extensions.Logging;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterNodeHandlerTests;

internal sealed class FilterNodeHandlerWithtChildrenTests : TestBase
{
    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.AndKey)]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.OrKey)]
    public void HandleWithOneCondition_WithOneChild_ReturnsExpectedResultWithoutParanthesis(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatiorStr,
        string value,
        string filterLogicStr
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterNode childFilterNode = new(
        [
            new FilterCondition(fieldName, filterOperator, value)
        ]);
        FilterLogic filterLogic = filterLogicStr!;
        FilterNode filterNode = new(
            filterLogic,
            [new FilterCondition(fieldName, filterOperator, value)],
            [childFilterNode]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();

        string expectedConditionString = $"x.{fieldName} {expectedOperatiorStr} \"{value}\"";
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
            conditionHandler.Received(2).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            result.ShouldBe($"{expectedConditionString} {filterLogic.Operator} {expectedConditionString}");
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.AndKey)]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.OrKey)]
    public void HandleWithOneCondition_WithChildAndGrandChild_ReturnsExpectedResultWithoutParanthesis(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatiorStr,
        string value,
        string filterLogicStr
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterNode grandChildNode = new(
        [new(fieldName, filterOperator, value)]);
        FilterLogic filterLogic = filterLogicStr!;
        FilterNode childFilterNode = new(
            filterLogic,
            [],
            [grandChildNode]);
        FilterNode filterNode = new(
            filterLogic,
            [
                new(fieldName, filterOperator, value),
                new(fieldName, filterOperator, value),
            ],
            [childFilterNode]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();

        string expectedConditionString = $"x.{fieldName} {expectedOperatiorStr} \"{value}\"";
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
            conditionHandler.Received(3).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            result.ShouldBe($"{expectedConditionString} {filterLogic.Operator} {expectedConditionString} {filterLogic.Operator} {expectedConditionString}");
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.AndKey, "Age", FilterOperator.GtKey, ">", 30, FilterLogic.OrKey)]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.OrKey, "Age", FilterOperator.GtKey, ">", 30, FilterLogic.AndKey)]
    public void HandleWithOneCondition_WithOneChildHavingMultipleConditions_ReturnsExpectedResultWithParanthesis(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatiorStr,
        string value,
        string filterLogicStr,
        string fieldName2,
        string filterOperatorStr2,
        string expectedOperatiorStr2,
        int value2,
        string filterLogicStr2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterLogic filterLogic = filterLogicStr!;
        FilterLogic filterLogic2 = filterLogicStr2!;
        FilterNode childFilterNode = new(
            filterLogic2,
            [
                new(fieldName, filterOperator, value),
                new(fieldName2, filterOperator2, value2)
            ]);
        FilterNode filterNode = new(
            filterLogic,
            [
                new FilterCondition(fieldName, filterOperator, value),
            ],
            [childFilterNode]);
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
            conditionHandler.Received(2).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            conditionHandler.Received(1).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName2
                && c.Operator == filterOperator2
                && c.Value!.Equals(value2)));
            result.ShouldBe($"{expectedConditionString} {filterLogic.Operator} ({expectedConditionString} {filterLogic2.Operator} {expectedConditionString2})");
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.AndKey, "Age", FilterOperator.GtKey, ">", 30, FilterLogic.OrKey)]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", FilterLogic.OrKey, "Age", FilterOperator.GtKey, ">", 30, FilterLogic.AndKey)]
    public void HandleWithMultipleCondition_WithOneChildrenHavingMultipleConditions_ReturnsExpectedResultWithParanthesis(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatiorStr,
        string value,
        string filterLogicStr,
        string fieldName2,
        string filterOperatorStr2,
        string expectedOperatiorStr2,
        int value2,
        string filterLogicStr2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterLogic filterLogic = filterLogicStr!;
        FilterLogic filterLogic2 = filterLogicStr2!;
        FilterNode childFilterNode1 = new(
            filterLogic2,
            [
                new(fieldName, filterOperator, value),
                new(fieldName2, filterOperator2, value2)
            ]);
        FilterNode childFilterNode2 = new(
            filterLogic2,
            [
                new(fieldName, filterOperator, value),
                new(fieldName2, filterOperator2, value2)
            ]);
        FilterNode filterNode = new(
            filterLogic,
            [],
            [childFilterNode1, childFilterNode2]);
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
            conditionHandler.Received(2).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName
                && c.Operator == filterOperator
                && c.Value!.Equals(value)));
            conditionHandler.Received(2).Handle(Arg.Is<FilterCondition>(c
                => c.Field == fieldName2
                && c.Operator == filterOperator2
                && c.Value!.Equals(value2)));
            result.ShouldBe($"({expectedConditionString} {filterLogic2.Operator} {expectedConditionString2}) {filterLogic.Operator} ({expectedConditionString} {filterLogic2.Operator} {expectedConditionString2})");
        }
    }
}
