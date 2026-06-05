using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

using System.Globalization;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

[Category("HypermediaEngine")]
internal sealed class DateTimeFilterConditionHandlerTests : TestBase
{
    [Test]
    [TestCase(FilterOperator.EqKey, "==", "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.NeKey, "!=", "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.GteKey, ">=", "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.GtKey, ">", "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.LteKey, "<=", "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.LtKey, "<", "2026-05-27T00:00:00")]
    public void Handle_WhenDateTimeValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr,
        string valueStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new DateTimeFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        DateTime value = DateTime.Parse(valueStr);
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} \"{value.ToString(CultureInfo.CurrentCulture)}\"";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    [TestCase(FilterOperator.ContainsKey, "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.NotContainsKey, "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.StartsWithKey, "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.NotStartsWithKey, "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.EndsWithKey, "2026-05-27T00:00:00")]
    [TestCase(FilterOperator.NotEndsWithKey, "2026-05-27T00:00:00")]
    public void Handle_WhenDateTimeValueProvidedWithInvalidOperators_ThrowsNotSupportedException(
        string opStr,
        string valueStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        DateTimeFilterConditionHandler handler = new(nextHandler);
        FilterOperator op = opStr!;
        DateTime value = DateTime.Parse(valueStr);
        FilterCondition condition = new(fieldName, op, value);
        nextHandler.Handle(Arg.Any<FilterCondition>()).Throws(new NotSupportedException(
            $"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."));

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            NotSupportedException ex = Should.Throw<NotSupportedException>(() => handler.Handle(condition));
            nextHandler.Received(1).Handle(condition);
            ex.Message.ShouldBe($"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'.");
        }
    }
}
