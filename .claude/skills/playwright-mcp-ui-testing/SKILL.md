---
name: playwright-mcp-ui-testing
description: AI-driven browser UI testing using the Playwright MCP server. Guides the sqa-engineer to perform autonomous browser interactions, assertions, and evidence capture through the Playwright MCP tools. Use ONLY when the change involves UI or frontend components.
---

# playwright-mcp-ui-testing

This skill guides the `sqa-engineer` agent to perform AI-driven UI tests using the Playwright MCP server (`@playwright/mcp`). The agent uses MCP browser tools to navigate, interact, observe, and assert UI behavior autonomously — without writing coded test files. This skill is ONLY invoked when the software change involves UI or frontend components.

---

## Prerequisites

Before invoking this skill:
1. Confirm the change is UI/frontend-related. If not, skip this skill entirely.
2. Ensure the Playwright MCP server is configured in `.mcp.json`:
```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest"],
      "type": "stdio"
    }
  }
}
```
3. Ensure the application is running (or start it via `dotnet run` or Aspire AppHost).
4. Discover the service URL (check Aspire dashboard at `http://localhost:15888` or use configured `launchUrl`).

---

## Phase 0 — Context Load (silent)

1. Read `CLAUDE.md` to understand the component under test.
2. Read the spec file at `docs/specs/<feature-slug>.spec.md` if it exists.
3. Load persistent memory: `Skill("manage-memory", args: "sqa-engineer")`.
4. Identify which UI routes/components were changed by the software-engineer.

---

## Phase 1 — Test Case Design

Design UI test cases based on user flows and acceptance criteria before interacting with the browser. For each test case specify:
- **Scenario name** (maps to an AC ID if available, e.g., `[AC-2] Submit form with valid data`)
- **Starting URL**
- **Steps** (navigation, interactions, form inputs)
- **Expected outcome** (element visible, text present, URL changed, etc.)
- **Evidence** (screenshot filename)

---

## Phase 2 — AI-Driven Browser Execution

Execute each test case using the Playwright MCP tools. Follow the **SAA pattern** (Snapshot → Act → Snapshot) for every interaction:

### Core Tool Reference

| Tool | Purpose |
|------|---------|
| `browser_navigate` | Navigate to a URL |
| `browser_snapshot` | Capture accessibility tree (default mode — use this for assertions) |
| `browser_screenshot` | Capture screenshot as evidence |
| `browser_click` | Click an element by ARIA ref |
| `browser_type` | Type into a focused element |
| `browser_fill` | Fill an input field |
| `browser_press_key` | Press keyboard key (Enter, Tab, Escape, etc.) |
| `browser_select_option` | Select a dropdown option |
| `browser_check` / `browser_uncheck` | Check/uncheck checkboxes |
| `browser_wait_for_load_state` | Wait for network idle or DOM loaded |
| `browser_wait` | Wait for N milliseconds (use sparingly) |
| `browser_get_console_log` | Retrieve browser console logs |
| `browser_get_network_requests` | List intercepted network requests |
| `browser_evaluate` | Execute JavaScript (escape hatch — requires justification) |

### SAA Pattern (Mandatory for every interaction)

```
Step 1: browser_snapshot          → read current state, identify ARIA refs
Step 2: browser_click/type/fill   → perform the interaction
Step 3: browser_snapshot          → verify state changed as expected
```

Never skip the post-action snapshot — it is the assertion.

### Evidence Capture

After each test case (pass or fail), capture:
```
Tool: browser_screenshot
Args: { "filename": "tests/screenshots/<scenario-name>-<pass|fail>.png" }
```

Also capture console errors:
```
Tool: browser_get_console_log
```
Treat any `[error]` level entries as implicit test failures.

---

## Phase 3 — Test Report

Produce a structured test report:

```
## AI-Driven UI Test Report

### Application: <app name and URL>
### Date: <ISO date>

| Scenario | AC ID | Steps | Result | Evidence |
|----------|-------|-------|--------|----------|
| Submit valid form | AC-2 | 3 | ✅ PASS | screenshots/submit-valid-pass.png |
| Submit empty form | AC-3 | 2 | ❌ FAIL | screenshots/submit-empty-fail.png |

### Console Errors Found
- [None] / [list any errors]

### Network Errors Found
- [None] / [list any 4xx/5xx responses]

### Summary: N passed, M failed
```

---

## Rules and Constraints

- **Snapshot mode is preferred** over vision mode — cheaper, faster, no vision model required.
- **Never use `browser_evaluate` as a shortcut** to set state directly. Use actual UI flows. Document any unavoidable use in the test report.
- **Flaky timing**: always use `browser_wait_for_load_state` or `browser_wait` after navigation/form submission — never assume instant rendering.
- **ARIA anchoring**: if UI elements lack proper ARIA labels/roles, surface it as a bug to the software-engineer and request semantic markup fixes before retesting.
- **Screenshots are mandatory evidence** — never submit a test report without screenshot files.
- **This skill produces a report, not test code.** The `write-tests` skill produces `.cs` test files. This skill produces browser interaction results and a markdown report.

---

## Escape Hatches (Require Justification in Report)

- `browser_evaluate` / `browser_run_code_unsafe`: only when no combination of interaction tools can achieve the required state.
- Vision mode (`--vision` flag on the MCP server): only when testing visual rendering (CSS, layout, images).
