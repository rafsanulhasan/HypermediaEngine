using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionOtherTests
{
    private Faker _faker = null!;

    [SetUp]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    public void ToString_NullValueAndEqOp_ReturnsConditionString()
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
        condStr.ShouldBe(string.Empty);
    }

    [Test]
    public void ToString_ObjectValueAndEqOp_ReturnsConditionString()
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        FilterCondition condition = new(
            field,
            FilterOperator.Eq,
            JsonSerializer.SerializeToElement(new { name = _faker.Name.FullName() }));

        // Act
        NotSupportedException ex = Should.Throw<NotSupportedException>(() => condition.ToString());

        // Assert
        ex!.Message.ShouldStartWith("Unknown JSON type node");
    }
}
