using Microsoft.AspNetCore.Hosting;

namespace SynergyFx.Tests.IntegrationTests.Factory;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string connectionString = ResourceInitialization.Database.Container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DB", connectionString);
        builder.UseEnvironment("Test");
    }
}
