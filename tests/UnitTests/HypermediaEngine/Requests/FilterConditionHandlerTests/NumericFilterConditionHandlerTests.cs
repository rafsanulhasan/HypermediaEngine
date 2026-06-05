using NSubstitute.ExceptionExtensions;

using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.Tests.UnitTests.Abstractions;

using System.Numerics;

namespace SynergyFx.Tests.UnitTests.HypermediaEngine.Requests.FilterConditionHandlerTests;

[Category("HypermediaEngine")]
internal sealed class NumericFilterConditionHandlerTests : TestBase
{
    [Test]
    // UInt16
    [TestCase(FilterOperator.EqKey, "==", (ushort)1)]
    [TestCase(FilterOperator.NeKey, "!=", (ushort)1)]
    [TestCase(FilterOperator.GteKey, ">=", (ushort)1)]
    [TestCase(FilterOperator.LteKey, "<=", (ushort)1)]
    [TestCase(FilterOperator.GtKey, ">", (ushort)1)]
    [TestCase(FilterOperator.LtKey, "<", (ushort)1)]
    // Int16
    [TestCase(FilterOperator.EqKey, "==", (short)-1)]
    [TestCase(FilterOperator.NeKey, "!=", (short)-1)]
    [TestCase(FilterOperator.GteKey, ">=", (short)-1)]
    [TestCase(FilterOperator.LteKey, "<=", (short)-1)]
    [TestCase(FilterOperator.GtKey, ">", (short)-1)]
    [TestCase(FilterOperator.LtKey, "<", (short)-1)]

    // UInt32
    [TestCase(FilterOperator.EqKey, "==", 1U)]
    [TestCase(FilterOperator.NeKey, "!=", 1U)]
    [TestCase(FilterOperator.GteKey, ">=", 1U)]
    [TestCase(FilterOperator.LteKey, "<=", 1U)]
    [TestCase(FilterOperator.GtKey, ">", 1U)]
    [TestCase(FilterOperator.LtKey, "<", 1U)]
    // Int32
    [TestCase(FilterOperator.EqKey, "==", -1)]
    [TestCase(FilterOperator.NeKey, "!=", -1)]
    [TestCase(FilterOperator.GteKey, ">=", -1)]
    [TestCase(FilterOperator.LteKey, "<=", -1)]
    [TestCase(FilterOperator.GtKey, ">", -1)]
    [TestCase(FilterOperator.LtKey, "<", -1)]

    // UInt64
    [TestCase(FilterOperator.EqKey, "==", 1UL)]
    [TestCase(FilterOperator.NeKey, "!=", 1UL)]
    [TestCase(FilterOperator.GteKey, ">=", 1UL)]
    [TestCase(FilterOperator.LteKey, "<=", 1UL)]
    [TestCase(FilterOperator.GtKey, ">", 1UL)]
    [TestCase(FilterOperator.LtKey, "<", 1UL)]
    // Int64
    [TestCase(FilterOperator.EqKey, "==", -1L)]
    [TestCase(FilterOperator.NeKey, "!=", -1L)]
    [TestCase(FilterOperator.GteKey, ">=", -1L)]
    [TestCase(FilterOperator.LteKey, "<=", -1L)]
    [TestCase(FilterOperator.GtKey, ">", -1L)]
    [TestCase(FilterOperator.LtKey, "<", -1L)]

    // Single
    [TestCase(FilterOperator.EqKey, "==", 1f)]
    [TestCase(FilterOperator.NeKey, "!=", -1f)]
    [TestCase(FilterOperator.GteKey, ">=", 1f)]
    [TestCase(FilterOperator.LteKey, "<=", -1f)]
    [TestCase(FilterOperator.GtKey, ">", 1f)]
    [TestCase(FilterOperator.LtKey, "<", -1f)]

    // Double
    [TestCase(FilterOperator.EqKey, "==", 1d)]
    [TestCase(FilterOperator.NeKey, "!=", -1d)]
    [TestCase(FilterOperator.GteKey, ">=", 1d)]
    [TestCase(FilterOperator.LteKey, "<=", -1d)]
    [TestCase(FilterOperator.GtKey, ">", 1d)]
    [TestCase(FilterOperator.LtKey, "<", -1d)]
    public void Handle_WhenNumericValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString<T>(
        string opStr,
        string expectedOpStr,
        T value
    ) where T : INumber<T>
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new NumericFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} {value}";

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
    [TestCase(FilterOperator.GteKey, ">=")]
    [TestCase(FilterOperator.LteKey, "<=")]
    [TestCase(FilterOperator.GtKey, ">")]
    [TestCase(FilterOperator.LtKey, "<")]
    public void Handle_WhenDecimalValueProvidedWithValidOperators_ReturnsExpectedDynamicLinqString(
        string opStr,
        string expectedOpStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        decimal value = Faker.Random.Decimal();
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        IFilterConditionHandler handler = new NumericFilterConditionHandler(nextHandler);
        FilterOperator op = opStr!;
        FilterCondition condition = new(fieldName, op, value);
        string expectedResult = $"x.{fieldName} {expectedOpStr} {value}";

        // Act and Assert
        using (Assert.EnterMultipleScope())
        {
            string result = Should.NotThrow(() => handler.Handle(condition));
            result.ShouldBe(expectedResult);
            nextHandler.DidNotReceive().Handle(Arg.Any<FilterCondition>());
        }
    }

    [Test]
    // UInt16
    [TestCase(FilterOperator.ContainsKey, (ushort)1)]
    [TestCase(FilterOperator.NotContainsKey, (ushort)1)]
    [TestCase(FilterOperator.StartsWithKey, (ushort)1)]
    [TestCase(FilterOperator.NotStartsWithKey, (ushort)1)]
    [TestCase(FilterOperator.EndsWithKey, (ushort)1)]
    [TestCase(FilterOperator.NotEndsWithKey, (ushort)1)]
    // Int16
    [TestCase(FilterOperator.ContainsKey, (short)-1)]
    [TestCase(FilterOperator.NotContainsKey, (short)-1)]
    [TestCase(FilterOperator.StartsWithKey, (short)-1)]
    [TestCase(FilterOperator.NotStartsWithKey, (short)-1)]
    [TestCase(FilterOperator.EndsWithKey, (short)-1)]
    [TestCase(FilterOperator.NotEndsWithKey, (short)-1)]

    // UInt32
    [TestCase(FilterOperator.ContainsKey, 1U)]
    [TestCase(FilterOperator.NotContainsKey, 1U)]
    [TestCase(FilterOperator.StartsWithKey, 1U)]
    [TestCase(FilterOperator.NotStartsWithKey, 1U)]
    [TestCase(FilterOperator.EndsWithKey, 1U)]
    [TestCase(FilterOperator.NotEndsWithKey, 1U)]
    // Int32
    [TestCase(FilterOperator.ContainsKey, -1)]
    [TestCase(FilterOperator.NotContainsKey, -1)]
    [TestCase(FilterOperator.StartsWithKey, -1)]
    [TestCase(FilterOperator.NotStartsWithKey, -1)]
    [TestCase(FilterOperator.EndsWithKey, -1)]
    [TestCase(FilterOperator.NotEndsWithKey, -1)]

    // UInt64
    [TestCase(FilterOperator.ContainsKey, 1UL)]
    [TestCase(FilterOperator.NotContainsKey, 1UL)]
    [TestCase(FilterOperator.StartsWithKey, 1UL)]
    [TestCase(FilterOperator.NotStartsWithKey, 1UL)]
    [TestCase(FilterOperator.EndsWithKey, 1UL)]
    [TestCase(FilterOperator.NotEndsWithKey, 1UL)]
    // Int64
    [TestCase(FilterOperator.ContainsKey, -1L)]
    [TestCase(FilterOperator.NotContainsKey, -1L)]
    [TestCase(FilterOperator.StartsWithKey, -1L)]
    [TestCase(FilterOperator.NotStartsWithKey, -1L)]
    [TestCase(FilterOperator.EndsWithKey, -1L)]
    [TestCase(FilterOperator.NotEndsWithKey, -1L)]

    // Single
    [TestCase(FilterOperator.ContainsKey, 1F)]
    [TestCase(FilterOperator.NotContainsKey, 1F)]
    [TestCase(FilterOperator.StartsWithKey, 1F)]
    [TestCase(FilterOperator.NotStartsWithKey, 1F)]
    [TestCase(FilterOperator.EndsWithKey, 1F)]
    [TestCase(FilterOperator.NotEndsWithKey, 1F)]

    // Double
    [TestCase(FilterOperator.ContainsKey, 1d)]
    [TestCase(FilterOperator.NotContainsKey, 1d)]
    [TestCase(FilterOperator.StartsWithKey, 1d)]
    [TestCase(FilterOperator.NotStartsWithKey, 1d)]
    [TestCase(FilterOperator.EndsWithKey, 1d)]
    [TestCase(FilterOperator.NotEndsWithKey, 1d)]
    public void Handle_WhenNumericValueProvidedWithInvalidOperators_ThrowsNotSupportedException<T>(
        string opStr,
        T value
    ) where T : INumber<T>
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        NumericFilterConditionHandler handler = new(nextHandler);
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

    [Test]
    [TestCase(FilterOperator.ContainsKey)]
    [TestCase(FilterOperator.NotContainsKey)]
    [TestCase(FilterOperator.StartsWithKey)]
    [TestCase(FilterOperator.NotStartsWithKey)]
    [TestCase(FilterOperator.EndsWithKey)]
    [TestCase(FilterOperator.NotEndsWithKey)]
    public void Handle_WhenDecimalValueProvidedWithInvalidOperators_ThrowsNotSupportedException(
        string opStr)
    {
        // Arrange
        string fieldName = Faker.Random.AlphaNumeric(10);
        decimal value = Faker.Random.Decimal();
        IFilterConditionHandler nextHandler = Substitute.For<IFilterConditionHandler>();
        NumericFilterConditionHandler handler = new(nextHandler);
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
