using Microsoft.AspNetCore.Hosting;

namespace HypermediaEngine.IntegrationTests.Factory;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string connectionString = ResourceInitialization.Database.Container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DB", connectionString);
        builder.UseEnvironment("Test");
    }

    public override async ValueTask DisposeAsync()
    {
        await ResourceInitialization.Database.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
