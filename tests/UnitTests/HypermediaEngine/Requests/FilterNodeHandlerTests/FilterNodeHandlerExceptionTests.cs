using Microsoft.Extensions.Logging;

using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterNodeHandlerTests;

internal sealed class FilterNodeHandlerExceptionTests : TestBase
{
    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", "Age", FilterOperator.GtKey, ">", 30)]
    public void HandleMultipleChildrenWithOneEmptyChild_WithLogic_ReturnsExpectedResultIgnoringExceptions(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatorStr,
        string value,
        string fieldName2,
        string filterOperatorStr2,
        string expectedOperatorStr2,
        int value2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterNode emptyChild = new([]);
        FilterCondition condition = new(fieldName2, filterOperator2, value2);
        FilterNode child2 = new([condition]);
        FilterNode child3 = new(
            FilterLogic.And,
            []);
        FilterNode filterNode = new(
            FilterLogic.And,
            [
                new(fieldName, filterOperator, value),
            ],
            [
                emptyChild,
                child2,
                child3
            ]);

        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c => c.Field == fieldName && c.Operator == filterOperator && c.Value!.Equals(value)))
            .Returns(_ => $"{fieldName} {expectedOperatorStr} \"{value}\"");
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c => c.Field == fieldName2 && c.Operator == filterOperator2 && c.Value!.Equals(value2)))
            .Returns(_ => $"{fieldName2} {expectedOperatorStr2} {value2}");
        InvalidOperationException exception = new("Conditions or Children is required when a Logic operator is provided.");
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(filterNode));
            result.ShouldBe($"{fieldName} {expectedOperatorStr} \"{value}\" && {fieldName2} {expectedOperatorStr2} {value2}");
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Is<EventId>(e => e.Id == 0),
                Arg.Is<object>(o => o.ToString()!.Contains($"{child3}")),
                Arg.Is<InvalidOperationException>(e => e.Message == exception.Message),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "John", "Age", FilterOperator.GtKey, 30)]
    public void HandleMultipleConditions_WithoutLogic_ThrowsInvalidOperationException(
        string fieldName,
        string filterOperatorStr,
        string value,
        string fieldName2,
        string filterOperatorStr2,
        int value2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterNode filterNode = new(
            [
                new FilterCondition(fieldName, filterOperator, value),
                new FilterCondition(fieldName2, filterOperator2, value2),
            ]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            InvalidOperationException? invalidOperationException = Assert.Throws<InvalidOperationException>(() => handler.Handle(filterNode));
            invalidOperationException.ShouldNotBeNull();
            invalidOperationException.Message.ShouldBe("Logic operator (And/Or) must be specified when combining multiple conditions or child nodes.");
        }
    }

    [Test]
    [TestCase("Name", FilterOperator.EqKey, "==", "John", "Age", FilterOperator.GtKey, 30)]
    public void HandleMultipleConditionsThatThrowsExceptions_WithLogic_ReturnsExpectedResultIgnoringExceptions(
        string fieldName,
        string filterOperatorStr,
        string expectedOperatorStr,
        string value,
        string fieldName2,
        string filterOperatorStr2,
        int value2
    )
    {
        // Arrange
        FilterOperator filterOperator = filterOperatorStr!;
        FilterOperator filterOperator2 = filterOperatorStr2!;
        FilterCondition condition = new(fieldName2, filterOperator2, value2);
        FilterNode filterNode = new(
            FilterLogic.And,
            [
                new FilterCondition(fieldName, filterOperator, value),
                condition,
            ]);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c => c.Field == fieldName && c.Operator == filterOperator && c.Value!.Equals(value)))
            .Returns(_ => $"{fieldName} {expectedOperatorStr} \"{value}\"");
        NotSupportedException exception = new($"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'.");
        conditionHandler
            .Handle(Arg.Is<FilterCondition>(c => c.Field == fieldName2 && c.Operator == filterOperator2 && c.Value!.Equals(value2)))
            .Throws(x => exception);
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(filterNode));
            result.ShouldBe($"{fieldName} {expectedOperatorStr} \"{value}\"");
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Is<EventId>(e => e.Id == 0),
                Arg.Is<object>(o => o.ToString()!.Contains($"{condition}")),
                Arg.Is<NotSupportedException>(e => e.Message == exception.Message),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }

    [Test]
    [TestCase(FilterLogic.AndKey)]
    [TestCase(FilterLogic.OrKey)]
    public void HandleNoConditions_WithLogic_ThrowsInvalidOperationException(
        string filterLogicStr
    )
    {
        // Arrange
        FilterLogic filterLogic = filterLogicStr!;
        FilterNode filterNode = new(
            filterLogic,
            []);
        IFilterConditionHandler conditionHandler = Substitute.For<IFilterConditionHandler>();
        ILogger<FilterNodeHandler> logger = Substitute.For<ILogger<FilterNodeHandler>>();
        FilterNodeHandler handler = new(conditionHandler, logger);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            InvalidOperationException? invalidOperationException = Assert.Throws<InvalidOperationException>(() => handler.Handle(filterNode));
            invalidOperationException.ShouldNotBeNull();
            invalidOperationException.Message.ShouldBe("Conditions or Children is required when a Logic operator is provided.");
        }
    }
}
