---
name: unit-testing-howto
description: Reusable how-to reference for writing TUnit unit tests in HypermediaEngine — attributes, assertion style, Bogus, TUnit.Mocks, project structure, InternalsVisibleTo.
metadata:
  type: project
---

# TUnit Unit Testing How-To — HypermediaEngine

## Project structure

Two separate test projects under `tests/`:

| Project        | Path                                          | Purpose                                      |
|----------------|-----------------------------------------------|----------------------------------------------|
| UnitTests      | `tests/UnitTests/UnitTests.csproj`            | Pure unit tests; no I/O, no HTTP, no DB      |
| IntegrationTests | `tests/IntegrationTests/IntegrationTests.csproj` | Full HTTP pipeline + real Postgres via Testcontainers |

`UnitTests` is for pure unit tests — no I/O, no HTTP, no DB.

---

## Unit tests — TUnit conventions

### Test runner attributes

```csharp
[Test]                          // marks a test method (replaces [TestCase] / [Fact])
[Arguments(value1, value2)]     // single parameterised row — stacks multiple for data-driven tests
[Arguments(values: [a, b, c])]  // array-form when passing multiple values including defaults
[Before(Test)]                  // per-test setup (replaces [SetUp])
[After(Test)]                   // per-test teardown (replaces [TearDown])
[Before(Class)]                 // per-class setup (replaces [OneTimeSetUp])
[After(Class)]                  // per-class teardown (replaces [OneTimeTearDown])
```

Test classes are `public sealed class` with no base class for unit tests.

---

## Assertions — TUnit.Assertions.Should

All assertions are `async` (return a `Task` that must be awaited):

```csharp
await actual.Should().BeEqualTo(expected);
await actual.Should().NotBeNull();
await actual.Should().BeNull();
await actual.Should().BeTrue();
await actual.Should().BeFalse();
await actual.Should().BeEmpty();
await actual.Should().HaveCount(n);
await actual.Should().ContainKey(key);
await actual.Should().StartWith("prefix");
```

`Assert.Multiple()` groups multiple assertions so all run even if one fails. The `using` is synchronous; the assertions inside are async:

```csharp
using (Assert.Multiple())
{
    await actual.Foo.Should().BeEqualTo(expected.Foo);
    await actual.Bar.Should().NotBeNull();
}
```

For "should not throw" wrap in `Assert.That(...).ThrowsNothing()`:

```csharp
await Assert.That(async () =>
{
    string result = sut.SomeMethod();
    await result.Should().BeEqualTo("expected");
}).ThrowsNothing();
```

For "should throw" use the synchronous helpers (synchronous throw only):

```csharp
SomeException ex = Assert.ThrowsExactly<SomeException>(() => sut.Method());
await ex.Message.Should().BeEqualTo("expected message");

// less strict — allows subclasses:
SomeException ex = Assert.Throws<SomeException>(() => sut.Method());
```

---

## Bogus — test data generation

Instantiate `Faker` in `[Before(Test)]`, not as a field initialiser, so each test gets a fresh instance:

```csharp
private Faker _faker;

[Before(Test)]
public void SetupTest() => _faker = new();
```

Common patterns:

```csharp
_faker.Random.AlphaNumeric(10)   // random string of length 10
_faker.Random.Int()              // random int
_faker.Random.Bool()
_faker.Random.Guid()
_faker.Person.FullName
_faker.Company.CompanyName()
```

---

## Mocking — TUnit.Mocks

Use TUnit.Mocks (not NSubstitute, not Moq) when a dependency must be faked in unit tests. Integration tests must never use mocks — use real infrastructure only.

---

## InternalsVisibleTo

`InternalsVisibleTo` for the test assemblies is configured in the production `.csproj` files via MSBuild `AssemblyAttribute` elements. Verify this is present before accessing `internal` types from tests — if it is missing, add it to the relevant `.csproj` rather than making the type `public`.
