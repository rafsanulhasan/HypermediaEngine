using HypermediaEngine.IntegrationTests.Resources;

namespace HypermediaEngine.IntegrationTests;

[SetUpFixture]
public sealed class ResourceInitialization
{
    public static PostgresDatabase Database { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task InitializeAssembly()
    {
        Database = new PostgresDatabase();
        await Database.StartAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeAssembly()
    {
        await Database.DisposeAsync();
    }
}
