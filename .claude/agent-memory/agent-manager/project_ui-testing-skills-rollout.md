# UI Testing Skills Rollout

## What was done

Three new UI testing skills were created and wired to `sqa-engineer` in session commit `86867c2` on branch `agentic-engineering`.

## New Skills

### playwright-mcp-ui-testing
- **Purpose:** AI-driven browser UI tests via `@playwright/mcp` MCP server — no coded test files produced
- **Key concept:** SAA pattern (Snapshot → Act → Snapshot) for every interaction; produces markdown test report + screenshot evidence files
- **Only invoke:** when the change is UI/frontend-related
- **Files:** `.agents/skills/playwright-mcp-ui-testing/SKILL.md`, `.claude/skills/playwright-mcp-ui-testing/SKILL.md`

### tunit-playwright-ui-testing
- **Purpose:** Coded C# end-to-end browser tests committed as `.cs` files, run via `dotnet test`
- **Stack:** TUnit (runner) + Microsoft.Playwright (browser automation)
- **Key patterns:** `ClassDataSource<PlaywrightFixture>(Shared = SharedType.PerTestSession)`, Page Object Model, `GetByRole`/`GetByLabel`/`GetByTestId` locators
- **Files:** `.agents/skills/tunit-playwright-ui-testing/SKILL.md`, `.claude/skills/tunit-playwright-ui-testing/SKILL.md`

### bunit-blazor-testing
- **Purpose:** Blazor component tests in isolation using bUnit (in-memory rendering, no real browser)
- **Stack:** bUnit 1.x + TUnit (runner) + TUnit.Assertions.Should + Bogus
- **Key patterns:** `TestContext` lifecycle (dispose after test class), `_ctx.Services` for DI mocking, `MarkupMatches()` for structural assertions, `SaveSnapshot()` for snapshot tests
- **Net10 caveat:** bUnit targets net8/net9; test project may need `<TargetFramework>net9.0</TargetFramework>` until bUnit adds net10 support
- **Files:** `.agents/skills/bunit-blazor-testing/SKILL.md`, `.claude/skills/bunit-blazor-testing/SKILL.md`

## Agent Updates

### sqa-engineer (both platforms)
- Three new skill entries appended after `csharp-architecture-testing` in Skills section
- Browser Testing section replaced: now documents dual-path (Playwright MCP preferred, docker-mcp-gateway fallback) with pointers to the three new skills
- Copilot file: replaced the inline Tool Selection Guidance bullets with concise skill pointer list

### triage-agent (both platforms)
- Added `## SDLC Workflow` section after `## Behavioral Principles`
- Documents standard SDLC execution order diagram
- Documents the three-way parallel post-software-engineer handoff:
  - Subagent 1: AI-driven UI tests (conditional on UI/frontend change)
  - Subagent 2: Code-driven tests (with nested parallel unit/integration/UI suite spawning)
  - Subagent 3: Documentation writer
- Documents code-reviewer handoff artifacts

## Naming Conventions Reinforced
- Skill directories: kebab-case under `.agents/skills/<name>/` and `.claude/skills/<name>/`
- Skill file: always `SKILL.md` (uppercase)
- Both locations must be created simultaneously — portability rule
