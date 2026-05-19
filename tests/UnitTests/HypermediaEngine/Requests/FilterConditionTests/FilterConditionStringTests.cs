using HypermediaEngine.Requests.Filtering;

using System.Text;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionStringTests
{
    private Faker _faker;

    [Before(Test)]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    [Arguments(values: [FilterOperator.EqKey, "==" ])]
    [Arguments(values: [FilterOperator.NeKey, "!=" ])]
    [Arguments(values: [FilterOperator.ContainsKey, "like", true, true ])]
    [Arguments(values: [FilterOperator.StartsWithKey, "like", false, true ])]
    [Arguments(values: [FilterOperator.EndsWithKey, "like", true, false ])]
    public async Task ToString_StringValueWithCompatibleOp_ReturnsConditionString(
        string opStr,
        string expectedOpStr,
        bool withStarting = false,
        bool withEnding = false)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        StringBuilder sb = new();
        string fieldValue = _faker.Person.FullName;
        if (withStarting) sb.Append('%');
        sb.Append(_faker.Person.FullName);
        if (withEnding)
            sb.Append('%');
        string expectedFieldValue = sb.ToString();
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonElement.Parse($"\"{fieldValue}\""));

        // Act and Assert
        using (Assert.Multiple())
        {
            await Assert.That(async () =>
            {
                string condStr = condition.ToString();
                await condStr.Should().BeEqualTo($"{field} {expectedOpStr} \"{expectedFieldValue}\"");
            }).ThrowsNothing();
        }
    }

    [Test]
    [Arguments(FilterOperator.GtKey)]
    [Arguments(FilterOperator.GteKey)]
    [Arguments(FilterOperator.LtKey)]
    [Arguments(FilterOperator.LteKey)]
    [Arguments(FilterOperator.InKey)]
    [Arguments(FilterOperator.NotInKey)]
    public async Task ToString_StringValueAndNonCompatipleOps_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        string fieldValue = _faker.Person.FullName;
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonElement.Parse($"\"{fieldValue}\""));

        // Act and Assert
        using (Assert.Multiple())
        {
            NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() => condition.ToString());
            await ex.Message.Should().BeEqualTo($"Unsupported combination: The type of the value with '{op}' operator");
        }
    }
}
