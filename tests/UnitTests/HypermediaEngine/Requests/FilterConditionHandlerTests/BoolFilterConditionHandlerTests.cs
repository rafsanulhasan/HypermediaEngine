using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

[Category("HypermediaEngine")]
internal sealed class BoolFilterConditionHandlerTests : TestBase
{
    [Test]
    [TestCase(FilterOperator.EqKey, "==", true)]
    [TestCase(FilterOperator.EqKey, "==", false)]
    [TestCase(FilterOperator.NeKey, "!=", true)]
    [TestCase(FilterOperator.NeKey, "!=", false)]
    public void Handle_WhenBoolValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr,
        bool value)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new BoolFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} {value.ToString()?.ToLower()}";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    [TestCase(FilterOperator.GteKey, true)]
    [TestCase(FilterOperator.GteKey, false)]
    [TestCase(FilterOperator.LteKey, true)]
    [TestCase(FilterOperator.LteKey, false)]
    [TestCase(FilterOperator.GtKey, true)]
    [TestCase(FilterOperator.GtKey, false)]
    [TestCase(FilterOperator.LtKey, true)]
    [TestCase(FilterOperator.LtKey, false)]
    [TestCase(FilterOperator.ContainsKey, true)]
    [TestCase(FilterOperator.ContainsKey, false)]
    [TestCase(FilterOperator.NotContainsKey, true)]
    [TestCase(FilterOperator.NotContainsKey, false)]
    [TestCase(FilterOperator.StartsWithKey, true)]
    [TestCase(FilterOperator.StartsWithKey, false)]
    [TestCase(FilterOperator.NotStartsWithKey, true)]
    [TestCase(FilterOperator.NotStartsWithKey, false)]
    [TestCase(FilterOperator.EndsWithKey, true)]
    [TestCase(FilterOperator.EndsWithKey, false)]
    [TestCase(FilterOperator.NotEndsWithKey, true)]
    [TestCase(FilterOperator.NotEndsWithKey, false)]
    public void Handle_WhenBoolValueProvidedWithInvalidOperators_ThrowsNotSupportedException(
        string opStr,
        bool value)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        BoolFilterConditionHandler handler = new(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        nextHandler.Handle(Arg.Any<FilterCondition>()).Throws(new NotSupportedException(
            $"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."));

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            NotSupportedException ex = Should.Throw<NotSupportedException>(() => handler.Handle(condition));
            ex.Message.ShouldBe($"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'.");
        }
    }
}
