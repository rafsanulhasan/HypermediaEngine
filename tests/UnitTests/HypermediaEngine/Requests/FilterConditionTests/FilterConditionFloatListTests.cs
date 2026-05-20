using HypermediaEngine.Requests.Filtering;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionFloatListTests
{
    private Faker _faker = null!;

    [SetUp]
    public void SetupTest()
    {
        _faker = new();
    }

    [Test]
    [TestCase(FilterOperator.InKey, "in")]
    [TestCase(FilterOperator.NotInKey, "not in")]
    public void ToString_FloatListValueWithCompatibleOperators_ReturnsConditionString(
        string opStr,
        string expectedOp)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        List<float> fieldValues = [.. Enumerable.Range(1, 5).Select(_ => _faker.Random.Float())];
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            fieldValues);

        // Act
        string condStr = condition.ToString();

        // Assert
        condStr.ShouldBe($"{field} {expectedOp} ({string.Join(", ", fieldValues)})");
    }

    [Test]
    [TestCase(FilterOperator.ContainsKey)]
    [TestCase(FilterOperator.StartsWithKey)]
    [TestCase(FilterOperator.EndsWithKey)]
    [TestCase(FilterOperator.EqKey)]
    [TestCase(FilterOperator.NeKey)]
    [TestCase(FilterOperator.GteKey)]
    [TestCase(FilterOperator.GtKey)]
    [TestCase(FilterOperator.LteKey)]
    [TestCase(FilterOperator.LtKey)]
    public void ToString_FloatListValueAndUnsupportedOp_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        List<float> fieldValue = [.. Enumerable.Range(1, 5).Select(_ => _faker.Random.Float())];
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            fieldValue);

        // Act
        NotSupportedException ex = Should.Throw<NotSupportedException>(() => condition.ToString());

        // Assert
        ex!.Message.ShouldBe($"Unsupported combination: The type of the value with '{op}' operator");
    }
}
