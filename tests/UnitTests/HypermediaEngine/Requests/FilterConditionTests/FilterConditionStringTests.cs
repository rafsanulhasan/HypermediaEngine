using HypermediaEngine.Requests.Filtering;

using System.Text;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests.FilterConditionTests;

public sealed class FilterConditionStringTests
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
    [TestCase(FilterOperator.ContainsKey, "like", true, true)]
    [TestCase(FilterOperator.StartsWithKey, "like", false, true)]
    [TestCase(FilterOperator.EndsWithKey, "like", true, false)]
    public void ToString_StringValueWithCompatibleOp_ReturnsConditionString(
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

        // Act
        string condStr = condition.ToString();

        // Assert
        condStr.ShouldBe($"x.{field} {expectedOpStr} \"{expectedFieldValue}\"");
    }

    [Test]
    [TestCase(FilterOperator.GtKey)]
    [TestCase(FilterOperator.GteKey)]
    [TestCase(FilterOperator.LtKey)]
    [TestCase(FilterOperator.LteKey)]
    [TestCase(FilterOperator.InKey)]
    [TestCase(FilterOperator.NotInKey)]
    public void ToString_StringValueAndNonCompatipleOps_ThrowsNotSupportedException(string opStr)
    {
        // Arrange
        string field = _faker.Random.AlphaNumeric(10);
        string fieldValue = _faker.Person.FullName;
        FilterOperator op = opStr!;
        FilterCondition condition = new(
            field,
            op,
            JsonElement.Parse($"\"{fieldValue}\""));

        // Act
        NotSupportedException ex = Should.Throw<NotSupportedException>(() => condition.ToString());

        // Assert
        ex!.Message.ShouldBe($"Unsupported combination: The type of the value with '{op}' operator");
    }
}
