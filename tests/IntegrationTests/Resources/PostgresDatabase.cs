using Bogus;

using Testcontainers.PostgreSql;

using TUnit.Core.Interfaces;

namespace HypermediaEngine.IntegrationTests.Resources;

public sealed class PostgresDatabase : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", new Faker().Company.CompanyName())
        .WithPortBinding(5432)
        .Build();

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return Container.StartAsync();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return Container.DisposeAsync();
    }
}
