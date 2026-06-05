namespace SynergyFx.Tests.UnitTests.Abstractions;

internal abstract class TestBase
{
    public Faker Faker {  get; protected set; }

    [SetUp]
    public virtual void SetUp()
    {
        Faker = new();
    }
}
