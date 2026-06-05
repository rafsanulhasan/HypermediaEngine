using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

internal sealed class InvalidFilterConditionHandlerTests : TestBase
{
    [Test]
    [TestCase(FilterOperator.LtKey, 1)]
    [TestCase(FilterOperator.EqKey, true)]
    public void Handle_WhenCalled_ThrowsNotSupportedException(
        string filterOperatorStr,
        object value)
    {
        // Arrange
        IFilterConditionHandler handler = new InvalidFilterConditionHandler();
        FilterOperator filterOperator = filterOperatorStr!;
        FilterCondition condition = new("Field", filterOperator, value);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            Should
                .Throw<NotSupportedException>(() => handler.Handle(condition))
                .Message
                .ShouldBe($"The filter operator '{filterOperatorStr}' is not supported for type '{value.GetType().Name}'.");
        }
    }

    [Test]
    [TestCase(FilterOperator.LtKey, 1)]
    [TestCase(FilterOperator.EqKey, true)]
    public void HandleString_WhenCalled_ThrowsNotSupportedException(
        string filterOperatorStr,
        object value)
    {
        // Arrange
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new StringFilterConditionHandler(nextHandler);
        FilterOperator filterOperator = filterOperatorStr!;
        FilterCondition condition = new("Field", filterOperator, value);

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            Should.NotThrow(() => handler.Handle(condition));
            nextHandler.Received(1).Handle(condition);
        }
    }
}
