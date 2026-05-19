---
description: GlobalUsings.cs convention for test projects in HypermediaEngine
globs: "tests/**/*.cs"
alwaysApply: false
---

# Test GlobalUsings Convention

- Both test projects (`UnitTests` and `IntegrationTests`) use a `GlobalUsings.cs` file for shared, project-wide imports.
- Add new project-wide `global using` directives to that file rather than repeating them at the top of individual test files.
- Both `GlobalUsings.cs` files must carry `[assembly: ExcludeFromCodeCoverage]`. Never remove this attribute.

## Example: UnitTests/GlobalUsings.cs

```csharp
global using Bogus;
global using System.Text.Json;
global using TUnit.Assertions.Should;
global using TUnit.Assertions.Should.Extensions;

[assembly: ExcludeFromCodeCoverage]
```

## Example: IntegrationTests/GlobalUsings.cs

```csharp
global using Aspire.Hosting.Testing;
global using TUnit.Assertions.Should;
global using TUnit.Assertions.Should.Extensions;
global using TUnit.Aspire;
global using TUnit.AspNetCore;

[assembly: ExcludeFromCodeCoverage]
```
