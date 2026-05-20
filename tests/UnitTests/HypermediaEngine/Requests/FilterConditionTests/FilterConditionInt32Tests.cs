using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionInt32Tests
{
    private Faker _faker = null!;

    [SetUp]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    [TestCase(FilterOperator.EqKey, "==")]
    [TestCase(FilterOperator.NeKey, "!=")]
    [TestCase(FilterOperator.GteKey, ">=")]
    [TestCase(FilterOperator.GtKey, ">")]
    [TestCase(FilterOperator.LteKey, "<=")]
    [TestCase(FilterOperator.LtKey, "<")]
    public void ToString_IntValueWithCompatibleOperators_ReturnsConditionString(
        string opStr,
        string expectedOp)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        int fieldValue = _faker.Random.Int();
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonSerializer.SerializeToElement(fieldValue));

        // Act
        string condStr = condition.ToString();

        // Assert
        condStr.ShouldBe($"{field} {expectedOp} {fieldValue}");
    }

    [Test]
    [TestCase(FilterOperator.ContainsKey)]
    [TestCase(FilterOperator.StartsWithKey)]
    [TestCase(FilterOperator.EndsWithKey)]
    [TestCase(FilterOperator.InKey)]
    [TestCase(FilterOperator.NotInKey)]
    public void ToString_IntValueAndUnsupportedOp_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        int fieldValue = _faker.Random.Int(max: -1);
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonSerializer.SerializeToElement(fieldValue));

        // Act
        NotSupportedException ex = Should.Throw<NotSupportedException>(() => condition.ToString());

        // Assert
        ex!.Message.ShouldBe($"Unsupported combination: The type of the value with '{op}' operator");
    }
}
