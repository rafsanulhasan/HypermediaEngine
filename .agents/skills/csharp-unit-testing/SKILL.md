---
name: csharp-unit-testing
description: Comprehensive guidance for writing C# unit tests using TUnit, TUnit.Mocks, Bogus, and TUnit.Assertions.Should with project conventions
---

# charp-unit-testing

This skill encodes best practices for writing C# unit tests in the HypermediaEngine project. It guides test implementation using a standardized stack: **TUnit** for the test framework, **TUnit.Mocks** for mocking, **Bogus** for test data generation, and **TUnit.Assertions.Should** for powerful assertions. It also reinforces project conventions like the `{ data, error }` return shape and Assert.Multiple() failure collection.

---

## Phase 0 — Context Load (silent)

1. Read `.claude/CLAUDE.md` and `.claude/rules/testing.md`
2. Invoke `Skill("manage-memory", args: "sqa-engineer")` to load persistent memory
3. Note the project conventions:
   - All API responses use `{ data, error }` shape
   - Test framework: TUnit
   - Mocking: TUnit.Mocks
   - Test data: Bogus
   - Assertions: TUnit.Assertions.Should

---

## Phase 1 — C# Unit Testing Conventions

### Testing Framework: TUnit

TUnit is the project's testing framework. Use it for:
- Creating test classes (mark with `[TestClass]`)
- Defining test methods (mark with `[Test]`)
- Test organization and discovery

**Why TUnit:** Lightweight, modern, integrates well with .NET 6+, and aligns with the HypermediaEngine architecture.

### Mocking Framework: TUnit.Mocks

TUnit.Mocks handles mock creation and behavior configuration. Always use it when a unit test needs to isolate the system under test (SUT) from its dependencies.

**Pattern 1: Create a mock**
```csharp
// Call .Mock() on the interface — the result IS the interface, no .Object needed
IRequestValidator mockValidator = IRequestValidator.Mock();
```

**Pattern 2: Configure return behavior**
```csharp
// Call the method with Any() for wildcard matching, then chain .Returns()
mockValidator.ValidateAsync(Any()).Returns(new ValidationResult { IsValid = true });

// Exact argument matching
mockValidator.GetUser(42).Returns(alice);
```

**Pattern 3: Verify invocation**
```csharp
// Call the method with the expected argument(s), then chain .WasCalled()
mockValidator.ValidateAsync(Any()).WasCalled(Times.Once);
mockValidator.GetUser(42).WasCalled();
```

**Pattern 4: Argument matching**
```csharp
Any()               // matches any value (like It.IsAny<T>())
42                  // exact value match
id => id > 0        // inline lambda predicate
```

**Why TUnit.Mocks:** Zero ceremony — the mock IS the interface. Setup and verification are expressed as plain method calls, not lambda expressions.

### Test Data Generation: Bogus

Bogus generates realistic test data. Always use it instead of hardcoded test data, especially for complex objects.

**Pattern 1: Create a Faker instance**
```csharp
Faker<User> userFaker = new()
    .RuleFor(u => u.Id, f => f.Random.Guid())
    .RuleFor(u => u.Email, f => f.Internet.Email())
    .RuleFor(u => u.Name, f => f.Name.FullName());

User testUser = userFaker.Generate();
```

**Pattern 2: Generate multiple instances**
```csharp
List<User> users = userFaker.Generate(5);
```

**Why Bogus:** Produces realistic, varied test data; reduces brittleness from hardcoded values; makes tests easier to read and maintain.

### Assertion Library: TUnit.Assertions.Should

TUnit.Assertions.Should provides readable, fluent assertions. Always use it instead of basic `Assert` statements.

**Pattern 1: Simple assertions**
```csharp
await result.Should().NotBeNull();
await result.Id.Should().BeEqualTo(expectedId);
await result.Name.Should().StartWith("Test");
```

**Pattern 2: Collection assertions**
```csharp
await result.Items.Should().HaveCount(3);
await result.Items.Should().Any(x => x.Status == "Active");
await result.Items.Should().All(x => x.CreatedAt <= DateTime.UtcNow);
```

Note: `.Any(predicate)` and `.All(predicate)` are hand-written in TUnit.Assertions.Should and take a plain `Func<TItem, bool>` — not nested `.Should()` chains.

**Pattern 3: Exception assertions (Act + Assert merged)**

When asserting that a method call throws an exception, assign the call to a lambda and assert on it. This merges Act and Assert into one focused block.

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

**Pattern 4: And/Or chaining**
```csharp
await value
    .Should().BeEqualTo(5)
    .And.NotBeEqualTo(7)
    .And.BeBetween(1, 10);

await statusCode
    .Should().BeEqualTo(200)
    .Or.BeEqualTo(201);
```
Note: Do not mix `.And` and `.Or` in the same chain — TUnit will throw `MixedAndOrAssertionsException`.

**Pattern 5: Justification**
```csharp
await score.Should().BeGreaterThan(70).Because("passing grade is required");
```

**Why TUnit.Assertions.Should:** Reads like English, provides rich failure messages, fluent syntax integrates seamlessly with TUnit.

### Assert.Multiple: Collect All Failures

Always wrap multiple assertions in `Assert.Multiple()` to collect all failures before reporting. This is critical for debugging — you see all problems at once instead of fixing them one-by-one.

**Pattern: Use Assert.Multiple()**
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

If any assertion fails, the block collects all failures and reports them together. This saves debugging time.

**Why Assert.Multiple():** Avoids stopping at the first failure; lets you see the full picture of what went wrong in a single test run. Native to TUnit assertion framework.

### The `{ data, error }` Return Shape

All HypermediaEngine API responses use a consistent shape:
```csharp
public record ApiResponse<T>(T? Data, string? Error);
```

**Pattern: Assert both fields**
```csharp
// Success case
ApiResponse<UserDto> response = await handler.HandleAsync(validRequest);
using (Assert.Multiple())
{
    await response.Data.Should().NotBeNull();
    await response.Error.Should().BeNull();
    await response.Data!.Id.Should().BeEqualTo(expectedId);
}

// Error case
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

Every test follows the **Arrange, Act, Assert** pattern to keep test logic clear and maintainable. This structure separates concerns and makes tests easier to understand at a glance.

**Arrange** — Set up dependencies, test data, and the system under test (SUT). Configure mocks, build test objects with Bogus, and prepare any preconditions needed.

**Act** — Call the method under test exactly once. This is a single line of code that invokes the SUT with the arranged inputs. Any logic here should belong in production code, not the test.

**Assert** — Verify that the result matches expectations using TUnit.Assertions.Should. Use `Assert.Multiple()` to collect all failures at once.

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

**Critical rule: one Act per test.** If a test needs to call the SUT more than once, split it into separate tests. Each test should verify a single behavior path.

---

## Phase 2 — Complete Example: Middleware Test

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
8. **Test behavior, not implementation** — tests must survive refactoring

Follow these patterns consistently across all test suites.
