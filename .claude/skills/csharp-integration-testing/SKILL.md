---
name: csharp-integration-testing
description: Comprehensive guidance for writing C# integration tests using TUnit, TestWebApplicationFactory, Testcontainers, Bogus, and TUnit.Assertions.Should — no mocks, real infrastructure
---

# csharp-integration-testing

This skill encodes best practices for writing C# integration tests in the HypermediaEngine project. Integration tests exercise the system end-to-end through real infrastructure — they do not use mocks. The standardized stack is **TUnit** for the test framework, **TUnit's `TestWebApplicationFactory<TEntryPoint>`** for ASP.NET Core hosting, **Testcontainers** for spinning up real databases, queues, caches, and other dependencies, **Bogus** for test data generation, and **TUnit.Assertions.Should** for assertions. Project conventions like the `{ data, error }` return shape and `Assert.Multiple()` failure collection still apply.

---

## Phase 0 — Context Load (silent)

1. Read `.claude/CLAUDE.md` and `.claude/rules/testing.md`
2. Invoke `Skill("manage-memory", args: "sqa-engineer")` to load persistent memory
3. Note the project conventions:
   - All API responses use `{ data, error }` shape
   - Test framework: TUnit
   - Integration test host: TUnit.AspNet (`TestWebApplicationFactory<TEntryPoint>`) — never plain `WebApplicationFactory`
   - Infrastructure: Testcontainers (PostgreSQL, Redis, Kafka, etc.) — never mocks in integration tests
   - Test data: Bogus
   - Assertions: TUnit.Assertions.Should

---

## Phase 1 — Integration vs. Unit Tests

Before writing any integration test, confirm it belongs in the integration suite:

| Concern | Unit test | Integration test |
| --- | --- | --- |
| Scope | One class, one method | HTTP request → middleware pipeline → DB → response |
| Dependencies | Mocked with `TUnit.Mocks` | **Real**, started via Testcontainers |
| Hosting | None — direct construction | `TestWebApplicationFactory<Program>` |
| Cost | Microseconds | Seconds (container startup amortised across session) |
| Failure signal | Logic regression | Wiring, configuration, contract, or schema regression |

**Rule:** if a test would still pass with the database swapped for an in-memory dictionary, it is a unit test — write it in the unit suite using `csharp-unit-testing`. Integration tests must exercise the real wire.

**No mocks in integration tests.** If you find yourself reaching for `IFoo.Mock()` in an integration test, you are writing the wrong kind of test. Stop, decide whether the behaviour belongs in the unit suite or whether the dependency needs a Testcontainer.

---

## Phase 2 — C# Integration Testing Conventions

### Test Hosting: TestWebApplicationFactory

Use `TUnit.AspNet`'s `TestWebApplicationFactory<TEntryPoint>` rather than the vanilla ASP.NET `WebApplicationFactory`. It gives you trace correlation, per-test logging, and `TestContext.Current` access for free.

**Pattern 1: Define the factory once per project**
```csharp
public sealed class HypermediaTestFactory : TestWebApplicationFactory<Program>
{
    [ClassDataSource<PostgresFixture>(Shared = SharedType.PerTestSession)]
    public required PostgresFixture Postgres { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            Dictionary<string, string?> overrides = new()
            {
                ["ConnectionStrings:Default"] = Postgres.ConnectionString,
            };
            config.AddInMemoryCollection(overrides);
        });
    }
}
```

**Pattern 2: Define a base class so every test inherits the factory**
```csharp
public abstract class IntegrationTestsBase : WebApplicationTest<HypermediaTestFactory, Program>
{
}
```

**Pattern 3: Per-test service overrides (use sparingly)**
```csharp
protected override void ConfigureTestServices(IServiceCollection services)
{
    // Replace clock so deterministic assertions are possible
    services.ReplaceService<IClock>(new FixedClock(DateTimeOffset.UnixEpoch));
}
```

**Why TestWebApplicationFactory:** It wires OpenTelemetry headers, captures HTTP exchanges, and routes server-side `ILogger` output to the originating test — without these, parallel test runs interleave logs and you cannot tell which test produced which line.

### Real Infrastructure: Testcontainers

Testcontainers spins up real Docker containers for the lifetime of the test session. **Never use mocks for the database, message broker, cache, or any other infrastructure** — that is the entire point of integration testing.

**Pattern 1: Wrap a container as an `IAsyncInitializer` / `IAsyncDisposable` fixture**
```csharp
public sealed class PostgresFixture : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("hypermedia_test")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync() => await Container.StartAsync();

    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}
```

**Pattern 2: Inject the fixture into the factory with `[ClassDataSource]`**
```csharp
[ClassDataSource<PostgresFixture>(Shared = SharedType.PerTestSession)]
public required PostgresFixture Postgres { get; init; }
```

`SharedType.PerTestSession` means the container starts once for the entire test run, not once per test class. Every test borrows the same instance.

**Pattern 3: Multi-container topology with a shared Docker network**
```csharp
public sealed class DockerNetwork : IAsyncInitializer, IAsyncDisposable
{
    public INetwork Instance { get; } = new NetworkBuilder()
        .WithName($"hypermedia-test-{Guid.NewGuid():N}")
        .Build();

    public async Task InitializeAsync() => await Instance.CreateAsync();

    public async ValueTask DisposeAsync() => await Instance.DisposeAsync();
}

public sealed class KafkaFixture : IAsyncInitializer, IAsyncDisposable
{
    [ClassDataSource<DockerNetwork>(Shared = SharedType.PerTestSession)]
    public required DockerNetwork Network { get; init; }

    public KafkaContainer Container { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Container = new KafkaBuilder()
            .WithNetwork(Network.Instance)
            .Build();
        await Container.StartAsync();
    }

    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}
```

TUnit resolves the dependency chain (network → kafka → factory) automatically — initialization runs in dependency order, disposal in reverse.

**Pattern 4: Per-test isolation on shared infrastructure**

Because `SharedType.PerTestSession` reuses the same database across tests, parallel tests will collide unless each test owns its own slice of state. Use TUnit's `GetIsolatedName()` / `GetIsolatedPrefix()` helpers from `WebApplicationTest`:

```csharp
public sealed class TodoApiTests : IntegrationTestsBase
{
    private string _schemaName = null!;

    protected override async Task SetupAsync()
    {
        _schemaName = GetIsolatedName("todos");      // e.g., "Test_42_todos"
        await Postgres.CreateSchemaAsync(_schemaName);
    }

    protected override void ConfigureTestConfiguration(IConfigurationBuilder config)
    {
        Dictionary<string, string?> overrides = new()
        {
            ["Database:Schema"] = _schemaName,
        };
        config.AddInMemoryCollection(overrides);
    }
}
```

**Golden rule:** if a resource is shared, every test must address its own slice of it (schema, table prefix, queue name, cache prefix, blob path). Otherwise parallel tests will flake.

**Why Testcontainers:** Mocks lie about what the database accepts; integration tests with the real engine catch broken migrations, missing indexes, dialect-specific SQL, contract violations between services, and serialization mismatches that mocks cannot see.

### Lifecycle Hooks: `[Before]` and `[After]`

TUnit's `[Before(scope)]` and `[After(scope)]` hooks replace NUnit's `[SetUp]`/`[TearDown]` family. They are method attributes — async-first, no naming convention required.

| Attribute | Scope | Runs | NUnit equivalent |
|---|---|---|---|
| `[Before(Test)]` | Per test | Before each test method | `[SetUp]` |
| `[After(Test)]` | Per test | After each test method | `[TearDown]` |
| `[Before(Class)]` | Per class | Once before all tests in the class | `[OneTimeSetUp]` |
| `[After(Class)]` | Per class | Once after all tests in the class | `[OneTimeTearDown]` |
| `[Before(Assembly)]` | Per assembly | Once for the whole assembly (static only) | `[SetUpFixture]` |
| `[After(Assembly)]` | Per assembly | Once for the whole assembly (static only) | `[SetUpFixture]` teardown |

**Pattern 1: Create and dispose a `TestWebApplicationFactory` at class scope**

```csharp
public sealed class TodoApiTests
{
    private HypermediaTestFactory _factory = null!;

    [Before(Class)]
    public async Task CreateFactory()
    {
        _factory = new HypermediaTestFactory();
        await _factory.InitializeAsync();
    }

    [After(Class)]
    public async Task DisposeFactory()
    {
        await _factory.DisposeAsync();
    }
}
```

**Pattern 2: Create and dispose a test container at class scope**

```csharp
public sealed class TodoRepositoryTests
{
    private PostgreSqlContainer _postgres = null!;

    [Before(Class)]
    public async Task StartPostgres()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("hypermedia_test")
            .Build();
        await _postgres.StartAsync();
    }

    [After(Class)]
    public async Task StopPostgres()
    {
        await _postgres.StopAsync();
        await _postgres.DisposeAsync();
    }
}
```

**Pattern 3: Inject resolved services in `[Before(Test)]`**

After the factory and container are live at class scope, use `[Before(Test)]` to resolve per-test dependencies from the factory's DI container and create a fresh HTTP client per test:

```csharp
public sealed class TodoApiTests
{
    private HypermediaTestFactory _factory = null!;
    private HttpClient _client = null!;
    private ITodoRepository _repository = null!;
    private string _schema = null!;

    [Before(Class)]
    public async Task CreateFactory()
    {
        _factory = new HypermediaTestFactory();
        await _factory.InitializeAsync();
    }

    [Before(Test)]
    public async Task SetupTest()
    {
        // Fresh client per test — prevents cookie/header state leaking between tests
        _client = _factory.CreateClient();

        // Resolve services from the factory's DI container
        _repository = _factory.Services.GetRequiredService<ITodoRepository>();

        // Per-test isolation: each test owns its own schema
        _schema = $"test_{TestContext.Current!.TestDetails.TestId:N}"[..16];
        await _factory.Postgres.CreateSchemaAsync(_schema);
    }

    [After(Test)]
    public async Task CleanupTest()
    {
        _client.Dispose();
        await _factory.Postgres.DropSchemaAsync(_schema);
    }

    [After(Class)]
    public async Task DisposeFactory()
    {
        await _factory.DisposeAsync();
    }
}
```

**Pattern 4: Full explicit stack — container + factory + per-test injection**

Use when you own the full lifecycle and are not inheriting from `WebApplicationTest<T>`:

```csharp
public sealed class CreateTodoEndpointTests
{
    private PostgreSqlContainer _postgres = null!;
    private HypermediaTestFactory _factory = null!;
    private HttpClient _client = null!;
    private string _schema = null!;

    [Before(Class)]
    public async Task StartInfrastructure()
    {
        // Container first
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("hypermedia_test")
            .Build();
        await _postgres.StartAsync();

        // Factory second — wires the container's connection string in
        _factory = new HypermediaTestFactory(_postgres.GetConnectionString());
        await _factory.InitializeAsync();
    }

    [Before(Test)]
    public async Task SetupTest()
    {
        // Per-test: isolated schema + fresh client + resolved services
        _schema = $"test_{Guid.NewGuid():N}"[..16];
        await _postgres.CreateSchemaAsync(_schema);
        _client = _factory.CreateClient();
    }

    [After(Test)]
    public async Task TeardownTest()
    {
        _client.Dispose();
        await _postgres.DropSchemaAsync(_schema);
    }

    [After(Class)]
    public async Task StopInfrastructure()
    {
        // Dispose in reverse order: factory first, container second
        await _factory.DisposeAsync();
        await _postgres.StopAsync();
        await _postgres.DisposeAsync();
    }
}
```

**Pattern 5: Assembly-wide shared resources (static hooks)**

Assembly hooks must be `static` methods in a `static class`. Use them only for truly global resources that every test class in the assembly shares:

```csharp
public static class TestAssemblyHooks
{
    public static INetwork SharedNetwork { get; private set; } = null!;

    [Before(Assembly)]
    public static async Task CreateSharedNetwork()
    {
        SharedNetwork = await new NetworkBuilder()
            .WithName($"hypermedia-test-{Guid.NewGuid():N}")
            .CreateAsync();
    }

    [After(Assembly)]
    public static async Task DestroySharedNetwork()
    {
        await SharedNetwork.DisposeAsync();
    }
}
```

**When to use `[Before/After]` vs. `[ClassDataSource]`**

| Scenario | Prefer |
|---|---|
| Session-shared infrastructure (one container for all classes) | `[ClassDataSource<T>(Shared = SharedType.PerTestSession)]` |
| Class-owned infrastructure (container per test class) | `[Before(Class)]` / `[After(Class)]` |
| Per-test setup (client, schema, resolved services) | `[Before(Test)]` / `[After(Test)]` |
| Assembly-wide global resources | `[Before(Assembly)]` / `[After(Assembly)]` (static) |

**Disposal order rule:** always dispose in reverse construction order — if you created factory after container, dispose factory before container.

### Aspire-Hosted Stacks

If the system under test is composed via .NET Aspire, prefer `TUnit.Aspire`'s `AspireFixture<TAppHost>` — it builds the AppHost, boots every container, waits for health checks, and exposes typed HTTP clients for each resource.

**Pattern: Direct fixture usage**
```csharp
[ClassDataSource<AspireFixture<Projects.HypermediaEngine_AppHost>>(Shared = SharedType.PerTestSession)]
public sealed class WeatherApiTests(AspireFixture<Projects.HypermediaEngine_AppHost> fixture)
{
    [Test]
    public async Task GetForecast_ReturnsOk()
    {
        HttpClient client = fixture.CreateHttpClient("apiservice");
        HttpResponseMessage response = await client.GetAsync("/weatherforecast");
        await response.StatusCode.Should().BeEqualTo(HttpStatusCode.OK);
    }
}
```

**Pattern: Customise the fixture**
```csharp
public sealed class HypermediaAspireFixture : AspireFixture<Projects.HypermediaEngine_AppHost>
{
    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(3);

    protected override IEnumerable<string> ResourcesToRemove() =>
        ["pgadmin", "redisinsight", "seq"];

    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());
    }
}
```

**Why Aspire fixtures:** the AppHost already encodes the production topology — re-using it in tests removes drift between "what we test" and "what we ship".

### Test Data Generation: Bogus

Bogus still applies. Hardcoded seed data hides bugs and makes integration tests brittle.

```csharp
Faker<CreateTodoRequest> requestFaker = new Faker<CreateTodoRequest>()
    .RuleFor(r => r.Title, f => f.Lorem.Sentence(3))
    .RuleFor(r => r.DueDate, f => f.Date.FutureOffset());

CreateTodoRequest request = requestFaker.Generate();
```

When seeding the database directly, generate full aggregates with Bogus and persist them through the production repository — never via raw SQL. Round-tripping through the real persistence layer is part of what you are validating.

### Assertion Library: TUnit.Assertions.Should

Same library, same patterns as unit tests. Quick reference for integration-flavoured cases:

**Pattern 1: HTTP response assertions**
```csharp
await response.StatusCode.Should().BeEqualTo(HttpStatusCode.Created);
await response.Headers.Location.Should().NotBeNull();
```

**Pattern 2: Deserialised body**
```csharp
ApiResponse<TodoDto>? body = await response.Content.ReadFromJsonAsync<ApiResponse<TodoDto>>();
using (Assert.Multiple())
{
    await body.Should().NotBeNull();
    await body!.Data.Should().NotBeNull();
    await body.Error.Should().BeNull();
    await body.Data!.Title.Should().BeEqualTo(request.Title);
}
```

**Pattern 3: Side-effect verification**

Integration tests check observable side effects — never internal state.
```csharp
await using NpgsqlConnection conn = new(Postgres.ConnectionString);
await conn.OpenAsync();
long count = await conn.ExecuteScalarAsync<long>(
    "select count(*) from todos where id = @id", new { id = body.Data!.Id });
await count.Should().BeEqualTo(1);
```

**Pattern 4: Async exceptions** — same as unit tests
```csharp
Func<Task> act = async () => await client.GetAsync("/will-fail");
await act.Should().Throw<HttpRequestException>();
```

Use `.Should().Throw<>()` for both `Action` and `Func<Task>` — never `.ThrowAsync<>()`, never try/catch.

### Assert.Multiple: Collect All Failures

Always wrap related assertions in `Assert.Multiple()`. In integration tests this matters even more — a single test exercises many layers, and you want to see *all* the things that broke at once instead of fixing them one by one across long container-startup cycles.

```csharp
using (Assert.Multiple())
{
    await response.StatusCode.Should().BeEqualTo(HttpStatusCode.OK);
    await body.Data.Should().NotBeNull();
    await body.Error.Should().BeNull();
    await body.Data!.Items.Should().HaveCount(3);
}
```

### The `{ data, error }` Return Shape

Every API response — including those reached via the HTTP pipeline — must be asserted on both `Data` and `Error`. No exceptions for integration tests.

```csharp
ApiResponse<TodoDto>? body = await response.Content.ReadFromJsonAsync<ApiResponse<TodoDto>>();
using (Assert.Multiple())
{
    await body.Should().NotBeNull();
    await body!.Data.Should().BeNull();
    await body.Error.Should().NotBeNullOrEmpty();
    await body.Error.Should().Contain("validation failed");
}
```

### Test Naming Convention

Use the same `Method_Scenario_Outcome` shape as unit tests, but the "method" is usually the HTTP verb + route:

```csharp
[Test]
public async Task PostTodos_WithValidPayload_PersistsAndReturnsCreated() { /* ... */ }

[Test]
public async Task GetTodos_WhenSchemaIsEmpty_ReturnsEmptyCollection() { /* ... */ }
```

If the suite ties to a numbered AC, prefix the display name with the AC ID (`[AC-3] PostTodos_...`).

### Test Structure: Arrange, Act, Assert (AAA)

Same discipline as unit tests:

- **Arrange** — seed any required state through the production repository (not raw SQL); generate request payloads with Bogus; build the HTTP client from the factory.
- **Act** — issue **one** HTTP call (or one operation) per test.
- **Assert** — verify the response, then verify the persisted side effect, then verify any emitted events.

**One Act per test.** If you find yourself making two HTTP calls "to set up state, then verify", the first call is arrangement — split it into a separate test or do the seeding directly through a fixture helper.

---

## Phase 3 — Complete Example: Endpoint Integration Test

```csharp
namespace HypermediaEngine.IntegrationTests;

[TestClass]
public sealed class CreateTodoEndpointTests : IntegrationTestsBase
{
    [Test]
    public async Task PostTodos_WithValidPayload_PersistsAndReturnsCreated()
    {
        // Arrange
        string schema = GetIsolatedName("todos");
        await Factory.Postgres.CreateSchemaAsync(schema);

        Faker<CreateTodoRequest> requestFaker = new Faker<CreateTodoRequest>()
            .RuleFor(r => r.Title, f => f.Lorem.Sentence(3))
            .RuleFor(r => r.DueDate, f => f.Date.FutureOffset());
        CreateTodoRequest request = requestFaker.Generate();

        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/todos", request);

        // Assert
        ApiResponse<TodoDto>? body = await response.Content.ReadFromJsonAsync<ApiResponse<TodoDto>>();
        using (Assert.Multiple())
        {
            await response.StatusCode.Should().BeEqualTo(HttpStatusCode.Created);
            await response.Headers.Location.Should().NotBeNull();
            await body.Should().NotBeNull();
            await body!.Data.Should().NotBeNull();
            await body.Error.Should().BeNull();
            await body.Data!.Title.Should().BeEqualTo(request.Title);
        }

        // Side-effect verification — the row really exists in Postgres
        await using NpgsqlConnection conn = new(Factory.Postgres.ConnectionString);
        await conn.OpenAsync();
        long persistedCount = await conn.ExecuteScalarAsync<long>(
            $"select count(*) from {schema}.todos where id = @id",
            new { id = body!.Data!.Id });
        await persistedCount.Should().BeEqualTo(1);
    }

    [Test]
    public async Task PostTodos_WithEmptyTitle_ReturnsValidationError()
    {
        // Arrange
        CreateTodoRequest request = new() { Title = string.Empty, DueDate = DateTimeOffset.UtcNow.AddDays(1) };
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/todos", request);

        // Assert
        ApiResponse<TodoDto>? body = await response.Content.ReadFromJsonAsync<ApiResponse<TodoDto>>();
        using (Assert.Multiple())
        {
            await response.StatusCode.Should().BeEqualTo(HttpStatusCode.BadRequest);
            await body.Should().NotBeNull();
            await body!.Data.Should().BeNull();
            await body.Error.Should().NotBeNullOrEmpty();
            await body.Error.Should().Contain("Title");
        }
    }
}
```

---

## Phase 4 — Operational Concerns

### Performance & Parallelism

- Container startup is the dominant cost. Always use `SharedType.PerTestSession` for infrastructure fixtures.
- TUnit runs tests in parallel by default. Pair `PerTestSession` containers with per-test isolation (`GetIsolatedName`, `GetIsolatedPrefix`) — never assume serial execution.
- If a test must run alone, mark it `[NotInParallel]` — but treat that as a bug to fix, not a default.

### CI/CD

- Integration tests require a running Docker daemon on the build agent.
- Set `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` in CI when using `AspireFixture` without trusted certificates.
- Increase `ResourceTimeout` to 2–5 minutes on shared CI runners.

### Diagnostics

- `WatchResourceLogs("apiservice")` streams a container's logs into the test output — invaluable when a test fails and you need to know what the app saw.
- `EnableHttpExchangeCapture = true` records every request/response pair the test issued — assert against `HttpCapture!.Last` when the response object alone is not enough.

### What NOT to do

- Do **not** seed test data with raw SQL when a repository exists — round-trip through the real code path.
- Do **not** mock anything in an integration test. If you need a deterministic clock or a fake email sender, use `ConfigureTestServices` to replace it with a real test double, not a mock.
- Do **not** share mutable state between tests via static fields — use TUnit's isolation helpers instead.
- Do **not** skip `Assert.Multiple()` because "the test only has two assertions". The discipline is uniform.

---

## Phase 5 — Key Takeaways

When writing C# integration tests for HypermediaEngine:

1. **Host with `TestWebApplicationFactory<Program>`** — never the vanilla ASP.NET factory.
2. **Use Testcontainers for real infrastructure** — Postgres, Redis, Kafka, all of it.
3. **No mocks** — if you need one, the test belongs in the unit suite.
4. **Share containers across the session, isolate state per test** — `PerTestSession` + `GetIsolatedName`.
5. **Use Aspire fixtures** when the system is already composed with Aspire.
6. **Generate test data with Bogus** — round-trip seeds through the production repository.
7. **Assert with TUnit.Assertions.Should** — `.Should().Throw<>()` for both sync and async.
8. **Wrap related assertions in `Assert.Multiple()`** — see all failures from one container cycle.
9. **Always assert both `Data` and `Error`** — the `{ data, error }` shape is mandatory end-to-end.
10. **Verify side effects** — read the database, check emitted events, inspect captured exchanges.

Follow these patterns consistently across all integration suites.
