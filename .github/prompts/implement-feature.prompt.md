---
description: "Structured feature implementation workflow for HypermediaEngine. Takes an architecture design document or system design output and produces working, tested, convention-compliant code committed to source control."
agent: "agent"
argument-hint: "Path to architecture design document or describe what to implement"
---

# Operating Methodology

You implement features in five phases. Complete each phase fully before advancing. Never expand scope beyond the stated feature.

---

## Phase 0 — Context Load (silent, no user interaction)

Before writing any code:

1. Read `CLAUDE.md` to internalize all conventions.
2. Read the architecture design document or system design spec passed as the argument.
3. Locate the test project using Glob. Read existing tests to understand the testing patterns in use.
4. Read the files you will modify or create in full — never write adjacent to code you have not read.
5. If acceptance criteria (ACs) exist, list them explicitly — each AC must be addressable by a test.

---

## Phase 1 — Implementation Planning

Before writing any code, produce a plan (do not skip this even for small features):

1. List every new file to create (path, type, responsibility).
2. List every existing file to modify (path, what changes and why).
3. Confirm the DI registration plan — new components registered correctly (lifetime, extension method, order).
4. Confirm no captive dependency risks.
5. Identify the build order: which component must be built before others can compile.

Present the plan in one concise table. Do not proceed until the plan is internally consistent.

---

## Phase 2 — Implementation

Implement each component in build order. For each file written or modified:

### Convention Checklist

- [ ] Explicit type declarations: `FileStream stream = new();` not `var stream = new FileStream()`
  - Exception: `Stream stream = new FileStream()` when declared type is base/interface
  - Exception: `IEnumerable<T> items = new List<T>()` when upcasting to interface
- [ ] `await using` for any type that implements `IAsyncDisposable`
- [ ] All boundary-crossing operations return `{ data, error }` — no raw `throw` across component lines
- [ ] No stack trace in any response body — catch at the boundary, log internally, return sanitized error
- [ ] `ILogger<T>` injected via DI — no `Console.Write*` calls
- [ ] All dependencies are constructor-injected abstractions — no `new ConcreteService()` inside components
- [ ] No captive dependencies: scoped services not injected into singletons
- [ ] No extension method conversions on `QueryableHelpers` — keep C# extension blocks

After writing each file, re-read it to confirm the checklist passes before moving on.

---

## Phase 3 — Build Verification

```
dotnet build
```

Fix all compilation errors before continuing. Do not skip this phase.

---

## Phase 4 — Test Verification

### Run existing tests

```
dotnet test
```

All previously passing tests must still pass. A change that breaks existing tests is a regression — fix the regression before continuing.

### Add tests for new logic

If the feature introduces new public methods or branches, add tests to the test project. Tests must follow project conventions (xUnit, three-part names, `{ data, error }` assertions on both fields).

### Mutation testing

```
dotnet stryker
```

For each surviving mutant on new code paths:

1. Understand what logic the mutant changed.
2. Add or strengthen a test that observably fails when that mutation is present.
3. Re-run `dotnet stryker` to confirm the mutant is killed.

Leave a comment for any mutant that is intentionally not killed (logging-only code, architecturally unreachable path) — justify each one explicitly.

---

## Phase 5 — Commit

Stage only the files changed for this feature. Commit with a message that:

- Starts with a verb: "Add", "Implement", "Refactor", "Fix"
- Names the component: "Add IRequestValidator middleware"
- States the why if non-obvious: "...to enforce schema constraints before handler dispatch"

Example:
```
git add <specific files>
git commit -m "Add IRequestValidator middleware to enforce schema constraints before handler dispatch"
```

---

## Quality Gate

Do not mark the feature complete until:

- [ ] `dotnet build` exits with 0 errors
- [ ] `dotnet test` exits with 0 failures
- [ ] `dotnet stryker` produces no surviving mutants on new logic (or each survivor is justified in a comment)
- [ ] Every new interface and public method follows the `{ data, error }` return shape
- [ ] No stack traces can escape to a client
- [ ] All conventions from the checklist in Phase 2 are satisfied
