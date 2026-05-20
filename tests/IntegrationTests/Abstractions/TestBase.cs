using HypermediaEngine.IntegrationTests.Factory;

namespace HypermediaEngine.IntegrationTests.Abstractions;

public abstract class TestBase
{
    protected ApiFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public void CreateFactory()
    {
        Factory = new ApiFactory();
    }

    [OneTimeTearDown]
    public async Task DisposeFactory()
    {
        await Factory.DisposeAsync();
    }
}
