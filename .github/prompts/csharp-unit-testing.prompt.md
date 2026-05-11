---
description: "Comprehensive guidance for writing C# unit tests using TUnit, TUnit.Mocks, Bogus, and TUnit.Assertions.Should with project conventions"
agent: "agent"
argument-hint: "Describe the test you're writing or paste the component to be tested"
---

# charp-unit-testing

## Operating Methodology

You guide test implementation in three phases. Complete each phase fully before advancing. This skill provides comprehensive reference material for test frameworks, mocking patterns, test data generation, and assertion techniques aligned with HypermediaEngine conventions.

---

## Phase 0 — Context Load (silent, no user interaction)

Before providing guidance:

1. Read `CLAUDE.md` and project testing rules to understand conventions
2. Load persistent memory for the sqa-engineer agent
3. Internalize the project's testing stack:
   - Test framework: **TUnit** (`[TestClass]`, `[Test]`)
   - Mocking: **TUnit.Mocks** (`IFoo.Mock()`)
   - Test data: **Bogus** (`Faker<T>`)
   - Assertions: **TUnit.Assertions.Should** (`.Should()` API)
   - Failure collection: **Assert.Multiple()**
4. Understand the mandatory `{ data, error }` response shape

---

## Phase 1 — Testing Conventions Reference

### Testing Framework: TUnit

TUnit is the project's testing framework. Use it to organize and discover tests.

**Create a test class:**
```csharp
[TestClass]
public class UserHandlerTests
{
}
```

**Define a test method:**
```csharp
[Test]
public async Task HandleAsync_WithValidRequest_ReturnsData()
{
}
```

**Why TUnit:** Lightweight, modern, and integrates seamlessly with .NET 6+.

### Mocking Framework: TUnit.Mocks

TUnit.Mocks isolates the system under test (SUT) from its dependencies.

**Create a mock:**
```csharp
// Call .Mock() on the interface — the result IS the interface, no .Object needed
IRequestValidator mockValidator = IRequestValidator.Mock();
```

**Configure return behavior:**
```csharp
// Call the method with Any() for wildcard matching, then chain .Returns()
mockValidator.ValidateAsync(Any()).Returns(new ValidationResult { IsValid = true });

// Exact argument matching
mockValidator.GetUser(42).Returns(alice);
```

**Verify invocation:**
```csharp
// Call the method with the expected argument(s), then chain .WasCalled()
mockValidator.ValidateAsync(Any()).WasCalled(Times.Once);
mockValidator.GetUser(42).WasCalled();
```

**Argument matching:**
```csharp
Any()               // matches any value
42                  // exact value match
id => id > 0        // inline lambda predicate
```

**Why TUnit.Mocks:** Zero ceremony — the mock IS the interface. Setup and verification are expressed as plain method calls, not lambda expressions.

### Test Data Generation: Bogus

Bogus generates realistic test data. Always use it instead of hardcoded values.

**Create a Faker:**
```csharp
Faker<User> userFaker = new()
    .RuleFor(u => u.Id, f => f.Random.Guid())
    .RuleFor(u => u.Email, f => f.Internet.Email())
    .RuleFor(u => u.Name, f => f.Name.FullName());

User testUser = userFaker.Generate();
```

**Generate multiple instances:**
```csharp
List<User> users = userFaker.Generate(5);
```

**Why Bogus:** Realistic, varied test data; reduces brittleness from hardcoded values.

### Assertion Library: TUnit.Assertions.Should

TUnit.Assertions.Should provides readable, fluent assertions. Always use it instead of basic `Assert` statements.

**Simple assertions:**
```csharp
await result.Should().NotBeNull();
await result.Id.Should().BeEqualTo(expectedId);
await result.Name.Should().StartWith("Test");
```

**Collection assertions:**
```csharp
await result.Items.Should().HaveCount(3);
await result.Items.Should().Any(x => x.Status == "Active");
await result.Items.Should().All(x => x.CreatedAt <= DateTime.UtcNow);
```

**Exception assertions (Act + Assert merged):**

When asserting that a method throws an exception, assign the call to a lambda and assert on it. This merges Act and Assert into one focused block.

**Async exception:**
```csharp
// Arrange
GetUserHandler handler = new(mockRepo);
GetUserRequest request = new() { Id = Guid.Empty };

// Act + Assert
Func<Task> act = async () => await handler.HandleAsync(request);
await act.Should().Throw<ArgumentException>();
```

**Sync exception:**
```csharp
// Arrange
RequestValidator validator = new();
HttpRequest invalidInput = new Faker<HttpRequest>().Generate();

// Act + Assert
Action act = () => validator.Validate(invalidInput);
await act.Should().Throw<ValidationException>();
```

**Critical rule:** Always use TUnit.Assertions.Should's `.Should().Throw<>()` — this works for both `Action` (sync) and `Func<Task>` (async) delegates. Never use `.ThrowAsync<>()` (it does not exist in TUnit.Assertions.Should) and never use try/catch.

**Note:** To assert exception messages, use `Assert.That()` — TUnit.Assertions.Should does not expose `.WithMessage()` on exception assertions.

**Why TUnit.Assertions.Should:** Reads like English, provides rich failure messages, fluent syntax integrates seamlessly with TUnit.

### Assert.Multiple: Collect All Failures

Wrap multiple assertions in `Assert.Multiple()` to see all failures at once instead of fixing them one-by-one.

```csharp
using (Assert.Multiple())
{
    await result.Data.Should().NotBeNull();
    await result.Error.Should().BeNull();
    await result.Data!.Id.Should().BeEqualTo(expectedId);
    await result.Data!.Name.Should().BeEqualTo(expectedName);
    await result.Data!.CreatedAt.Should().BeLessThanOrEqualTo(DateTime.UtcNow);
}
```

If any assertion fails, the block collects all failures and reports them together.

**Why Assert.Multiple():** Avoids stopping at the first failure; lets you see the full picture of what went wrong in one test run. Native to TUnit assertion framework.

### The `{ data, error }` Return Shape

All HypermediaEngine API responses use a consistent shape:
```csharp
public record ApiResponse<T>(T? Data, string? Error);
```

**Pattern: Assert both fields**

Success case:
```csharp
ApiResponse<UserDto> response = await handler.HandleAsync(validRequest);
using (Assert.Multiple())
{
    await response.Data.Should().NotBeNull();
    await response.Error.Should().BeNull();
    await response.Data!.Id.Should().BeEqualTo(expectedId);
}
```

Error case:
```csharp
ApiResponse<UserDto> errorResponse = await handler.HandleAsync(invalidRequest);
using (Assert.Multiple())
{
    await errorResponse.Data.Should().BeNull();
    await errorResponse.Error.Should().NotBeNullOrEmpty();
    await errorResponse.Error.Should().Contain("validation failed");
}
```

**Important:** Always assert BOTH `Data` and `Error` fields. Never verify only one side. This ensures the response shape is correct.

### Test Naming Convention

Use descriptive test names that explain the scenario and expected outcome:

```csharp
[Test]
public async Task ValidateAsync_WithValidRequest_ReturnsSuccessResponse()
{
    // Arrange
    RequestValidator validator = new();
    HttpRequest validRequest = new Faker<HttpRequest>()
        .RuleFor(r => r.Method, "GET")
        .Generate();

    // Act
    ApiResponse<ValidationResult> result = await validator.ValidateAsync(validRequest);

    // Assert
    using (Assert.Multiple())
    {
        await result.Data.Should().NotBeNull();
        await result.Error.Should().BeNull();
    }
}
```

### Test Structure: Arrange, Act, Assert (AAA)

Every test follows the **Arrange, Act, Assert** pattern.

**Arrange** — Set up dependencies, test data, and the system under test (SUT). Configure mocks with TUnit.Mocks, build test objects with Bogus, and prepare preconditions.

**Act** — Call the method under test exactly once. This is a single line of code. Any logic here should belong in production code, not the test.

**Assert** — Verify that the result matches expectations using TUnit.Assertions.Should and `Assert.Multiple()` to collect all failures.

**Critical rule: one Act per test.** If a test needs multiple calls, split it into separate tests. Each test verifies a single behavior path.

**Example:**
```csharp
[Test]
public async Task HandleAsync_WithValidRequest_ReturnsData()
{
    // Arrange
    IUserRepository mockRepo = IUserRepository.Mock();
    User expectedUser = new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid())
        .RuleFor(u => u.Name, f => f.Name.FullName())
        .Generate();
    mockRepo.GetByIdAsync(expectedUser.Id).Returns(expectedUser);

    GetUserHandler handler = new(mockRepo);

    // Act
    ApiResponse<UserDto> response = await handler.HandleAsync(new GetUserRequest { Id = expectedUser.Id });

    // Assert
    using (Assert.Multiple())
    {
        await response.Data.Should().NotBeNull();
        await response.Error.Should().BeNull();
        await response.Data!.Id.Should().BeEqualTo(expectedUser.Id);
        await response.Data!.Name.Should().BeEqualTo(expectedUser.Name);
    }
}
```

### Coding Conventions for Tests

- Explicit type declarations: `IRequestValidator mockValidator = IRequestValidator.Mock();` not `var`
- Target-typed new: `Faker<User> userFaker = new()`, `DefaultHttpContext httpContext = new()`
- File-scoped namespace: `namespace HypermediaEngine.Tests;` at the top of every test file
- No `var` anywhere in test code

---

## Phase 2 — Complete Example: Middleware Tests

Here is a complete example testing a middleware component using all conventions together:

```csharp
namespace HypermediaEngine.Tests;

[TestClass]
public class RequestValidationMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_WithValidRequest_CallsNext()
    {
        // Arrange
        IRequestValidator mockValidator = IRequestValidator.Mock();
        mockValidator.ValidateAsync(Any()).Returns(new ValidationResult { IsValid = true });

        RequestDelegate mockNext = RequestDelegate.Mock();
        RequestValidationMiddleware middleware = new(mockNext, mockValidator);

        Faker<HttpRequest> requestFaker = new()
            .RuleFor(r => r.Method, "POST")
            .RuleFor(r => r.Path, "/api/users");

        HttpRequest request = requestFaker.Generate();
        DefaultHttpContext httpContext = new();
        httpContext.Request.CopyFrom(request);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (Assert.Multiple())
        {
            mockValidator.ValidateAsync(Any()).WasCalled(Times.Once);
            mockNext.Invoke(Any<HttpContext>()).WasCalled(Times.Once);
        }
    }

    [Test]
    public async Task InvokeAsync_WithInvalidRequest_ReturnsErrorResponse()
    {
        // Arrange
        IRequestValidator mockValidator = IRequestValidator.Mock();
        mockValidator.ValidateAsync(Any()).Returns(new ValidationResult { IsValid = false, Error = "Missing required header" });

        RequestDelegate mockNext = RequestDelegate.Mock();
        RequestValidationMiddleware middleware = new(mockNext, mockValidator);

        HttpRequest request = new Faker<HttpRequest>()
            .RuleFor(r => r.Method, "POST")
            .Generate();

        DefaultHttpContext httpContext = new();
        httpContext.Request.CopyFrom(request);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (Assert.Multiple())
        {
            mockValidator.ValidateAsync(Any()).WasCalled(Times.Once);
            mockNext.Invoke(Any<HttpContext>()).WasCalled(Times.Never);
            await httpContext.Response.StatusCode.Should().BeEqualTo(400);
        }
    }
}
```

---

## Phase 3 — Key Takeaways

When writing C# unit tests for HypermediaEngine:

1. **Use TUnit** — it's the project's test framework
2. **Mock with TUnit.Mocks** — fluent, intuitive, and integrated with TUnit
3. **Generate test data with Bogus** — realistic, varied, maintainable
4. **Assert with TUnit.Assertions.Should** — readable, fluent, powerful
5. **Collect failures with Assert.Multiple()** — see all problems at once
6. **Always assert both Data and Error** — the `{ data, error }` shape is mandatory
7. **Name tests clearly** — explain scenario and expected outcome
8. **Follow the AAA pattern** — Arrange, Act (once), Assert
9. **No try/catch for exceptions** — use TUnit.Assertions.Should always
10. **Test behavior, not implementation** — tests must survive refactoring

Follow these patterns consistently across all test suites.
