using Bogus;

using Testcontainers.PostgreSql;

namespace HypermediaEngine.IntegrationTests.Resources;

public sealed class PostgresDatabase : IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", new Faker().Company.CompanyName())
        .WithPortBinding(5432, assignRandomHostPort: true)
        .Build();

    public Task StartAsync() => Container.StartAsync();

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync().ConfigureAwait(false);
        await Container.DisposeAsync().ConfigureAwait(false);
    }
}
