using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

[Category("HypermediaEngine")]
internal sealed class StringFilterConditionHandlerTests : TestBase
{
    [Test]
    [TestCase(FilterOperator.GteKey)]
    [TestCase(FilterOperator.LteKey)]
    [TestCase(FilterOperator.GtKey)]
    [TestCase(FilterOperator.LtKey)]
    public void Handle_WhenStringValueProvidedWithInvalidOperators_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        string value = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        StringFilterConditionHandler handler = new(nextHandler);
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

    [Test]
    [TestCase(FilterOperator.EqKey, "==", null)]
    [TestCase(FilterOperator.NeKey, "!=", null)]
    public void Handle_WhenStringNullValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr,
        string? value)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new StringFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} null";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    [TestCase(FilterOperator.EqKey, "==")]
    [TestCase(FilterOperator.NeKey, "!=")]
    public void Handle_WhenStringValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        string value = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new StringFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} \"{value}\"";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    [TestCase(FilterOperator.NotContainsKey, "Contains")]
    [TestCase(FilterOperator.NotStartsWithKey, "StartsWith")]
    [TestCase(FilterOperator.NotEndsWithKey, "EndsWith")]
    public void Handle_WhenStringValueProvidedWithValidNegativeCsharpOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        string value = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new StringFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"!x.{fieldName}.{expectedOpStr}(\"{value}\")";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    [TestCase(FilterOperator.ContainsKey, "Contains")]
    [TestCase(FilterOperator.StartsWithKey, "StartsWith")]
    [TestCase(FilterOperator.EndsWithKey, "EndsWith")]
    public void Handle_WhenStringValueProvidedWithValidPositiveCsharpOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        string value = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new StringFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName}.{expectedOpStr}(\"{value}\")";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }
}
