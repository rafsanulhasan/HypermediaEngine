---
name: integration-testing-howto
description: Reusable how-to reference for writing TUnit integration tests in HypermediaEngine — TestBase/WebApplicationTest pattern, ApiFactory, Testcontainers Postgres, no-mocks rule, disabled Aspire fixture.
metadata:
  type: project
---

# Integration Testing How-To — HypermediaEngine

## Project structure

Two separate test projects under `tests/`:

| Project        | Path                                          | Purpose                                      |
|----------------|-----------------------------------------------|----------------------------------------------|
| UnitTests      | `tests/UnitTests/UnitTests.csproj`            | Pure unit tests; no I/O, no HTTP, no DB      |
| IntegrationTests | `tests/IntegrationTests/IntegrationTests.csproj` | Full HTTP pipeline + real Postgres via Testcontainers |

`IntegrationTests` exercises the full HTTP pipeline and real Postgres via Testcontainers. It is not for isolated logic tests.

---

## Integration tests — TUnit.AspNetCore + Testcontainers

### Base class pattern

Every integration test class inherits from `TestBase`, which wraps `WebApplicationTest<ApiFactory, Program>`:

```csharp
// tests/IntegrationTests/Abstractions/TestBase.cs
public abstract class TestBase : WebApplicationTest<ApiFactory, Program> { }

// test class
internal sealed class MyEndpointTests : TestBase
{
    private HttpClient _httpClient;

    [Before(Test)]
    public async Task InitializeTest()
    {
        _httpClient = Factory.CreateClient();
    }

    [After(Test)]
    public void EndTest() => _httpClient.Dispose();
}
```

### Factory + Testcontainers Postgres

`ApiFactory` wires a real Postgres container and injects its connection string via environment variable:

```csharp
public sealed class ApiFactory : TestWebApplicationFactory<Program>
{
    [ClassDataSource<PostgresDatabase>(Shared = SharedType.PerTestSession)]
    public PostgresDatabase Database { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string connectionString = Database.Container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DB", connectionString);
        builder.UseEnvironment("Test");
    }
}
```

`SharedType.PerTestSession` means one Postgres container for the entire test session. Do not change it to `PerTest` — that makes startup prohibitively slow.

`PostgresDatabase` implements `IAsyncInitializer` + `IAsyncDisposable`:

```csharp
public sealed class PostgresDatabase : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", new Faker().Company.CompanyName())
        .WithPortBinding(5432)
        .Build();

    public Task InitializeAsync() => Container.StartAsync();
    public ValueTask DisposeAsync() => Container.DisposeAsync();
}
```

Note: `Faker().Company.CompanyName()` may produce names with characters illegal in Postgres identifiers. If container startup fails, replace with `new Faker().Random.AlphaNumeric(12)`.

### No mocks in integration tests

Integration tests must exercise real infrastructure end-to-end. Never inject mock services into `ApiFactory`. If a test would pass with the DB swapped for an in-memory dictionary, it belongs in `UnitTests` instead.

### Aspire-based fixture (currently disabled)

`tests/IntegrationTests/Fixtures/AspireFixture.cs` is fully commented out. The Aspire-based test path (`AspireFixture<Projects.Sample_AppHost>`) is not wired. Do not attempt to use it until it is uncommented and the Aspire host is properly configured.

---

## InternalsVisibleTo

`InternalsVisibleTo` for the test assemblies is configured in the production `.csproj` files via MSBuild `AssemblyAttribute` elements. Verify this is present before accessing `internal` types from tests — if it is missing, add it to the relevant `.csproj` rather than making the type `public`.
