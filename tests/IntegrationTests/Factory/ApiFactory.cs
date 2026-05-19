using HypermediaEngine.IntegrationTests.Resources;

using Microsoft.AspNetCore.Hosting;

namespace HypermediaEngine.IntegrationTests.Factory;

public sealed class ApiFactory : TestWebApplicationFactory<Program>
{
    [ClassDataSource<PostgresDatabase>(Shared = SharedType.PerTestSession)]
    public PostgresDatabase Database { get; init; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string connectionString = Database.Container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DB", connectionString);
        builder.UseEnvironment("Test");
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await Database.DisposeAsync();
        await base.DisposeAsync();
    }
}
