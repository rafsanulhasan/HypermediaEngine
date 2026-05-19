using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionOtherTests
{
    private Faker _faker;

    [Before(Test)]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    public async Task ToString_NullValueAndEqOp_ReturnsConditionString()
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        FilterCondition condition = new(
            field,
            FilterOperator.Eq,
            JsonElement.Parse("null"));

        // Act
        string condStr = condition.ToString();

        // Assert
        await condStr.Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task ToString_ObjectValueAndEqOp_ReturnsConditionString()
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        FilterCondition condition = new(
            field,
            FilterOperator.Eq,
            JsonSerializer.SerializeToElement( new { name = _faker.Name.FullName() }));

        // Act and Assert
        using (Assert.Multiple())
        {
            NotSupportedException ex = Assert.Throws<NotSupportedException>(() => condition.ToString());
            await ex.Message.Should().StartWith("Unknown JSON type node");
        }
    }
}
