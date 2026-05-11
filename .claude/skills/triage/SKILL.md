---
name: triage
description: "Task classification, decomposition, dependency mapping, and routing skill. Invoked by the triage-agent to break any user request into discrete, independently-routable work items with assigned priorities and agent chains."
---

You are executing the `triage` skill on behalf of the triage-agent. Your job is to take a user request and produce a structured work breakdown that the triage-agent can use to orchestrate a multi-agent workflow.

## Input

The calling agent passes the raw user prompt or a task description as `args`.

## Process

### Step 1 — Classify

Identify all task types present in the request:

| Type | Indicators | Default Priority |
|---|---|---|
| Feature | "Add X", "I want X", "we need X", new capability | P1–P2 |
| Bug Fix | Crash, incorrect behavior, failing test, exception, regression | P0–P1 |
| Security Fix | Auth bypass, data exposure, input validation gap, vulnerable dependency | P0 |
| TechDebt | Refactor, convention violation, cleanup, performance | P2–P3 |
| Release | "Ship", "release", "deploy", "publish to NuGet" | — |
| Question | Exploratory — "what would...", "how should we..." | — |

A single request may contain multiple types. Identify all present.

### Step 2 — Decompose

For complex requests (multiple types or >1 agent needed):

1. Identify atomic work items — each deliverable by a single, contiguous agent chain
2. Map dependencies: which items must complete before others can begin
3. Identify parallelization: which items share no dependency and can run concurrently    
4. Keep decomposition minimal — do not split a naturally-sequential flow into artificial fragments

For simple requests (one type, one agent, no dependencies): return a single-item plan.

### Step 3 — Assign Agent Chains

For each work item, select the tightest-fitting standard chain:

```
New feature (scope unknown)         → requirement-analyst → software-architect → system-engineer → software-engineer → sqa-engineer
New feature (design needed)         → software-architect → system-engineer → software-engineer → sqa-engineer
New feature (ready to build)        → software-engineer → sqa-engineer
Feature needs prioritization        → product-manager → (then appropriate build chain above)
Bug fix                             → software-engineer  (invoke fix-bug skill)
Bug fix with arch concern           → software-engineer → software-architect  (invoke architecture-review skill)
Security fix                        → /security-review skill directly
Tech debt / refactor                → software-engineer → code-reviewer
Tech debt needs prioritization      → product-manager → software-engineer → code-reviewer
Test coverage gap                   → sqa-engineer
Low-level design question           → system-engineer
Architecture question               → software-architect
Backlog / prioritization question   → product-manager
Release / milestone planning        → product-manager → deploy skill (downstream chain depends on items)
Agent / skill / command lifecycle   → agent-manager
Rules / instructions / hooks change → agent-manager
Agent memory prune / audit / refresh → agent-manager
```

### Step 4 — Set Priorities

Apply defaults, then check for overrides:

- Security Fix: always P0 — never downgrade
- Bug (regression or system blocker): P0; other bugs: P1
- Feature: P1 by default; P2 for cosmetic or low-impact work
- TechDebt: P2; raise to P1 only if it directly blocks a P0/P1 item

### Step 5 — Flag PM Consultation Requirement

Mark whether the PM should be consulted before execution begins:

- **Yes** — if any item is Feature or TechDebt
- **No** — if all items are Bug Fix (P0), Security Fix, Question, or Routing

## Output

Return the complete work breakdown to the triage-agent:

```
### Work Breakdown: <request summary>

#### Classification
<Type(s)> — <reason>

#### Work Items
| ID | Type | Priority | Description | Depends On | Parallel With | Agent Chain |
|---|---|---|---|---|---|---|
| WI-001 | Feature | P1 | Add X capability | — | — | req-analyst → sw-arch → sys-eng → sw-eng → sqa |
| WI-002 | Bug Fix | P0 | Fix Y crash in Z | — | — | software-engineer |

#### Execution Waves
- Wave 1 (parallel): WI-002 (P0 — immediate), WI-001 (pending PM priority confirmation)
- Wave 2 (after WI-001 completes): WI-003

#### PM Consultation Required
<Yes — Feature items WI-001 need prioritization before routing>
OR
<No — all items are P0 bug/security fixes, routing immediately>
```
