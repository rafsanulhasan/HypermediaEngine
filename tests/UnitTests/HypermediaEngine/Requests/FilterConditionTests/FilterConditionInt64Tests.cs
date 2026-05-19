using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionInt64Tests
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
    [Arguments(values: [FilterOperator.GteKey, ">="])]
    [Arguments(values: [FilterOperator.GtKey, ">"])]
    [Arguments(values: [FilterOperator.LteKey, "<="])]
    [Arguments(values: [FilterOperator.LtKey, "<"])]
    public async Task ToString_LongValueWithCompatibleOperators_ReturnsConditionString(
        string opStr,
        string expectedOp)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        long fieldValue = _faker.Random.Long();
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonSerializer.SerializeToElement(fieldValue));

        // Act and Assert
        using (Assert.Multiple())
        {
            await Assert.That(async () =>
            {
                string condStr = condition.ToString();
                await condStr.Should().BeEqualTo($"{field} {expectedOp} {fieldValue}");
            }).ThrowsNothing();
        }
    }

    [Test]
    [Arguments(FilterOperator.ContainsKey)]
    [Arguments(FilterOperator.StartsWithKey)]
    [Arguments(FilterOperator.EndsWithKey)]
    [Arguments(FilterOperator.InKey)]
    [Arguments(FilterOperator.NotInKey)]
    public async Task ToString_LongValueAndUnsupportedOp_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        long fieldValue = _faker.Random.Long(max: -1);
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonSerializer.SerializeToElement(fieldValue));

        // Act and Assert
        using (Assert.Multiple())
        {
            NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() => condition.ToString());
            await ex.Message.Should().BeEqualTo($"Unsupported combination: The type of the value with '{op}' operator");
        }
    }
}
