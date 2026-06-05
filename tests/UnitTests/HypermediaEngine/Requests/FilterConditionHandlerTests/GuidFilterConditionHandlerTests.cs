using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

[Category("HypermediaEngine")]
internal sealed class GuidFilterConditionHandlerTests : TestBase
{
    [Test]
    [TestCase(FilterOperator.EqKey, "==", "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NeKey, "!=", "00000000-0000-0000-0000-000000000000")]
    public void Handle_WhenGuidValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr,
        Guid value)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new GuidFilterConditionHandler(nextHandler);
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
    [TestCase(FilterOperator.GteKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.GteKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.LteKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.LteKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.GtKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.GtKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.LtKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.LtKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.ContainsKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.ContainsKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotContainsKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotContainsKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.StartsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.StartsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotStartsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotStartsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.EndsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.EndsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotEndsWithKey, "00000000-0000-0000-0000-000000000000")]
    [TestCase(FilterOperator.NotEndsWithKey, "00000000-0000-0000-0000-000000000000")]
    public void Handle_WhenGuidValueProvidedWithInvalidOperators_ThrowsNotSupportedException(
        string opStr,
        Guid value)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        GuidFilterConditionHandler handler = new(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        nextHandler.Handle(Arg.Any<FilterCondition>()).Throws(new NotSupportedException(
            $"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."));

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            NotSupportedException ex = Should.Throw<NotSupportedException>(() => handler.Handle(condition));
            ex.Message.ShouldBe($"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'.");
        }
    }
}
