---
name: tunit-playwright-ui-testing
description: Comprehensive guidance for writing coded end-to-end UI tests in C# using TUnit as the test framework with Playwright for browser automation. Covers NuGet setup, Page Object Model, browser lifecycle fixtures, async patterns, and best practices.
---

# tunit-playwright-ui-testing

This skill guides the `sqa-engineer` to write coded, repeatable UI tests in C# for web applications in HypermediaEngine using **TUnit** as the test runner and **Microsoft.Playwright** as the browser automation library.

> **When to use:** Use this skill for end-to-end tests that must be committed as `.cs` test files and run in CI via `dotnet test`. For one-time AI-driven exploratory browser tests, use `playwright-mcp-ui-testing` instead.

---

## Phase 0 — Context Load (silent)

1. Read `CLAUDE.md` and project conventions.
2. Load memory: `Skill("manage-memory", args: "sqa-engineer")`.
3. Identify the test project that will host Playwright tests (create a new `*.PlaywrightTests` project if one doesn't exist).

---

## Phase 1 — NuGet Setup

Add the following packages to the Playwright test project:

```xml
<PackageReference Include="Microsoft.Playwright" Version="1.44.*" />
<PackageReference Include="TUnit" Version="*" />
<PackageReference Include="TUnit.Assertions" Version="*" />
```

Run Playwright browser install after adding the package:
```bash
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
# or on Windows without pwsh:
dotnet run --project <test-project> -- playwright install chromium
```

---

## Phase 2 — Browser Lifecycle Fixture

Create a shared browser fixture using TUnit's `ClassDataSource` or a static initializer to manage browser lifetime across tests:

```csharp
using Microsoft.Playwright;
using TUnit.Core;

[ClassDataSource<PlaywrightFixture>(Shared = SharedType.PerTestSession)]
public class PlaywrightFixture : IAsyncInitializer, IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,  // set false for local debugging
            SlowMo = 0
        });
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _browser!.DisposeAsync();
        _playwright!.Dispose();
    }
}
```

---

## Phase 3 — Page Object Model

Encapsulate each page's interactions in a Page Object:

```csharp
public class WeatherPage(IPage page)
{
    private readonly IPage _page = page;
    private const string _url = "http://localhost:5000/weather";

    public async Task NavigateAsync()
        => await _page.GotoAsync(_url);

    public async Task<IReadOnlyList<ILocator>> GetForecastRowsAsync()
        => await _page.Locator("table tbody tr").AllAsync();

    public async Task<string> GetTitleAsync()
        => await _page.TitleAsync();
}
```

**Page Object rules:**
- One class per page or significant component.
- No assertions inside page objects — page objects return data; tests assert on it.
- Use `Locator` over `QuerySelector` — locators are lazy, retry-able, and more stable.
- Use `GetByRole`, `GetByLabel`, `GetByText`, `GetByTestId` before CSS selectors.

---

## Phase 4 — Test Class Structure

```csharp
using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

[TestClass]
public class WeatherPageTests(PlaywrightFixture playwright)
{
    private readonly WeatherPage _weatherPage = new(playwright.Page);

    [Test]
    [DisplayName("[AC-1] Weather page loads and shows forecast rows")]
    public async Task WeatherPage_WhenLoaded_ShowsForecastRows()
    {
        // Arrange
        await _weatherPage.NavigateAsync();

        // Act
        IReadOnlyList<ILocator> rows = await _weatherPage.GetForecastRowsAsync();

        // Assert
        await Assert.That(rows).HasCount().GreaterThan(0);
    }

    [Test]
    [DisplayName("[AC-2] Weather page title is correct")]
    public async Task WeatherPage_WhenLoaded_HasCorrectTitle()
    {
        await _weatherPage.NavigateAsync();
        string title = await _weatherPage.GetTitleAsync();
        await Assert.That(title).IsEqualTo("Weather Forecast");
    }
}
```

**Test class rules:**
- Use constructor injection to receive `PlaywrightFixture`.
- Each `[Test]` method tests exactly one behavior.
- Always `await` Playwright calls — never fire-and-forget.
- Include the AC ID in the display name.
- Use `Assert.Multiple()` when asserting multiple properties of the same result.

---

## Phase 5 — Async Patterns

Playwright is fully async. Follow these patterns:

```csharp
// ✅ Correct: await Playwright async APIs
await page.GotoAsync(url);
await page.ClickAsync("button[type=submit]");
ILocator input = page.GetByLabel("Email");
await input.FillAsync("user@example.com");

// ✅ Correct: use locators with built-in retry
ILocator successMsg = page.GetByText("Order confirmed");
await successMsg.WaitForAsync(new() { Timeout = 5000 });

// ❌ Wrong: sync .Result / .Wait()
page.GotoAsync(url).Result;

// ❌ Wrong: Thread.Sleep
Thread.Sleep(2000);
```

---

## Phase 6 — Running Tests

```bash
dotnet test --filter "FullyQualifiedName~PlaywrightTests"
```

For headed (visible browser) mode during local debugging:
```csharp
// In PlaywrightFixture.InitializeAsync():
Headless = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS") != "false"
```

Then run:
```bash
PLAYWRIGHT_HEADLESS=false dotnet test
# on Windows PowerShell:
$env:PLAYWRIGHT_HEADLESS="false"; dotnet test
```

---

## Phase 7 — CI Integration

In GitHub Actions, ensure Playwright browsers are installed before running tests:

```yaml
- name: Install Playwright browsers
  run: pwsh tests/YourProject.PlaywrightTests/bin/Release/net10.0/playwright.ps1 install --with-deps chromium
- name: Run Playwright tests
  run: dotnet test tests/YourProject.PlaywrightTests/
```

---

## Best Practices

| Do | Don't |
|----|-------|
| Use `GetByRole`, `GetByLabel`, `GetByTestId` | Use CSS selectors as the first choice |
| Use `Locator.WaitForAsync()` before assertions | Use arbitrary `browser_wait` delays |
| Keep page objects assertion-free | Put `Assert.That()` in page objects |
| Share browser via `SharedType.PerTestSession` | Create a new browser per test |
| Set `Headless = true` in CI | Leave headless mode hardcoded to `false` |
| Use `await using` for contexts | Forget to dispose contexts |
