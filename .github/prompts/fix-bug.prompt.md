---
description: "Structured bug investigation and fix workflow for HypermediaEngine. Takes a bug report or failing test and produces a minimal targeted fix that is verified, tested, and committed without introducing regressions."
agent: "agent"
argument-hint: "Describe the bug, paste the error/stack trace, or reference the failing test"
---

# Operating Methodology

You fix bugs in five phases. Complete each phase fully before advancing. Never expand scope beyond the stated bug.

---

## Phase 0 — Context Load (silent, no user interaction)

Before investigating anything:

1. Read `CLAUDE.md` to internalize conventions that the bug may violate.
2. Read the bug report, failing test output, or error description in full.
3. Identify which files and components are likely involved based on the report.
4. Read those files before forming any hypothesis.

---

## Phase 1 — Root Cause Investigation

**Rule: understand the cause before writing a single line of code.**

1. **Locate the failure point** — find the exact file, class, and method where the incorrect behavior originates. Use search; do not guess.

2. **Trace the call chain** — read from the entry point (middleware, filter, endpoint) down to the failure point. Identify every component the request passes through.

3. **Form a hypothesis** — state the root cause explicitly:
   - "The bug is in `ClassName.MethodName` at `path/to/file.cs:42` because `<reason>`."
   - If multiple hypotheses exist, list them and rank by probability.

4. **Verify the hypothesis** — check that the stated root cause is the *only* thing that needs to change to fix the bug. If fixing it would require changes in three unrelated places, you have identified a symptom, not the root cause.

5. **Present the root cause to the user** before writing any fix. State:
   - File and line number
   - What the code does now
   - What it should do instead
   - Why this is the root cause and not a symptom

Wait for confirmation before proceeding to Phase 2.

---

## Phase 2 — Minimal Fix Design

Design the smallest change that corrects the root cause:

- Change only what the root cause requires — no refactoring, no cleanup, no "while I'm here" improvements
- If the fix requires touching more than two files, the root cause analysis may be incomplete — revisit Phase 1
- If the bug exists because a convention was violated (missing `{ data, error }`, leaked exception, sync disposal), fix only that violation
- State exactly what will change: file path, method name, before → after

---

## Phase 3 — Implementation

Apply the fix following all project conventions:

### Convention Checklist

- [ ] Explicit type declarations (`FileStream stream = new();` not `var`)
- [ ] `await using` for disposable resources
- [ ] `{ data, error }` return shape at boundaries — no unhandled exceptions crossing component lines
- [ ] No stack trace in any response body — catch at boundary, log internally, return sanitized error
- [ ] Logger (`ILogger<T>`), not `Console.Write*`
- [ ] No new concrete dependencies — constructor-injected abstractions only

After applying the fix, re-read the changed file to confirm no unintended side effects.

---

## Phase 4 — Verify

### Build

```
dotnet build
```

Fix all compilation errors before continuing.

### Run tests

```
dotnet test
```

- All previously passing tests must still pass — a fix that breaks other tests is a regression
- If the bug had a reproducing test, it must now pass
- If no reproducing test exists, add one before marking the bug fixed

### Regression check

Read the changed code one final time and confirm:

- The fix is minimal — nothing changed outside the root cause
- No new `Console.Write*` calls, no new sync disposal, no new raw exception propagation
- The `{ data, error }` contract is preserved everywhere the fix touches

---

## Phase 5 — Commit

Commit only the files changed by the fix:

```
git add <specific files>
git commit -m "Fix <BugDescription> in <ComponentName>"
```

---

## Quality Gate

Do not mark the bug fixed until:

- [ ] Root cause stated with specific file and line number
- [ ] User confirmed the root cause before the fix was written
- [ ] `dotnet build` exits with 0 errors
- [ ] `dotnet test` exits with 0 failures
- [ ] A reproducing test exists and passes
- [ ] No previously passing test was broken
- [ ] Fix touches only what the root cause requires
