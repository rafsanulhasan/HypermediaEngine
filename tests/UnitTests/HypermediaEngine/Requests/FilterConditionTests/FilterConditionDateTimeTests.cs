using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionDateTimeTests
{
    private Faker _faker;

    [Before(Test)]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    [Arguments(values: [FilterOperator.EqKey, "=="])]
    [Arguments(values: [FilterOperator.NeKey, "!="])]
    public async Task ToString_DateTimeValueWithCompatibleOperators_ReturnsConditionString(
        string opStr,
        string expectedOp)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        DateTime fieldValue = _faker.Date.Future();
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            fieldValue);

        // Act and Assert
        using (Assert.Multiple())
        {
            await Assert.That(async () =>
            {
                string condStr = condition.ToString();
                await condStr.Should().BeEqualTo($"{field} {expectedOp} \"{fieldValue}\"");
            }).ThrowsNothing();
        }
    }

    [Test]
    [Arguments(FilterOperator.ContainsKey)]
    [Arguments(FilterOperator.StartsWithKey)]
    [Arguments(FilterOperator.EndsWithKey)]
    [Arguments(FilterOperator.InKey)]
    [Arguments(FilterOperator.NotInKey)]
    [Arguments(values: [FilterOperator.GteKey])]
    [Arguments(values: [FilterOperator.GtKey])]
    [Arguments(values: [FilterOperator.LteKey])]
    [Arguments(values: [FilterOperator.LtKey])]
    public async Task ToString_DateTimeValueAndUnsupportedOp_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        DateTime fieldValue = _faker.Date.Future();
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            fieldValue);

        // Act and Assert
        using (Assert.Multiple())
        {
            NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() => condition.ToString());
            await ex.Message.Should().BeEqualTo($"Unsupported combination: The type of the value with '{op}' operator");
        }
    }
}