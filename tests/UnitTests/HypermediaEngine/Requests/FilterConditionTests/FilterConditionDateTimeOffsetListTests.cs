using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionDateTimeOffsetListTests
{
    private Faker _faker;

    [Before(Test)]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    [Arguments(values: [FilterOperator.InKey, "in"])]
    [Arguments(values: [FilterOperator.NotInKey, "not in"])]
    public async Task ToString_DateTimeOffsetListValueWithCompatibleOperators_ReturnsConditionString(
        string opStr,
        string expectedOp)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        List<DateTimeOffset> fieldValues = [.. Enumerable.Range(1, 5).Select(_ => _faker.Date.RecentOffset())];
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            fieldValues);
        string fieldValueStr = string.Join(", ", fieldValues.Select(v => $"\"{v}\""));

        // Act and Assert
        using (Assert.Multiple())
        {
            await Assert.That(async () =>
            {
                string condStr = condition.ToString();
                await condStr.Should().BeEqualTo($"{field} {expectedOp} ({fieldValueStr})");
            }).ThrowsNothing();
        }
    }

    [Test]
    [Arguments(FilterOperator.ContainsKey)]
    [Arguments(FilterOperator.StartsWithKey)]
    [Arguments(FilterOperator.EndsWithKey)]
    [Arguments(FilterOperator.EqKey)]
    [Arguments(FilterOperator.NeKey)]
    [Arguments(FilterOperator.GteKey)]
    [Arguments(FilterOperator.GtKey)]
    [Arguments(FilterOperator.LteKey)]
    [Arguments(FilterOperator.LtKey)]
    public async Task ToString_DateTimeOffsetListValueAndUnsupportedOp_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        IEnumerable<DateTimeOffset> fieldValue = Enumerable.Range(1, 5).Select(_ => _faker.Date.RecentOffset());
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
