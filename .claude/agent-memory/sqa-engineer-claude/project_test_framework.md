---
name: project-test-framework
description: HypermediaEngine test project uses NUnit 4 + NSubstitute + Shouldly + Bogus (NOT TUnit). Tests are void or async Task with [TestCase] attribute. InternalsVisibleTo configured for HypermediaEngine.Tests.
metadata:
  type: project
---

The test project `tests/HypermediaEngine.Tests` uses:
- **NUnit 4** (not TUnit as documented in SQA skills) — `[TestCase]` attribute, `Assert.Multiple()`
- **NSubstitute** for mocking
- **Shouldly** for fluent assertions (note: Shouldly has its own `SortDirection` enum — qualify `HypermediaEngine.Requests.Sorting.SortDirection` fully or use alias)
- **Bogus** for test data generation

`InternalsVisibleTo` for `HypermediaEngine.Tests` is configured in `HypermediaEngine.AspNetCore.csproj` via AssemblyAttribute MSBuild element.

**Why:** The skills describe TUnit but the actual project uses NUnit. Always check `tests/Directory.Build.props` and `Directory.Packages.props` for actual framework.

**How to apply:** Use `[TestCase]` (not `[Test]`) for zero-argument tests matching existing convention. Use `Shouldly` assertions. Use `NSubstitute.Substitute.For<T>()` for mocks.
