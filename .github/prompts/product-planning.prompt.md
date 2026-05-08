---
description: "Product backlog management and release planning for HypermediaEngine. Handles five modes: reviewing backlog health, adding new work items, re-prioritizing, planning a release milestone, and updating item status. Invoke when asking what to work on next, adding a feature to the backlog, planning a release, or when a work item completes."
agent: "agent"
argument-hint: "Mode: review | add <item description> | prioritize | plan-release <version> | update <ITEM-NNN> <status>"
---

# Operating Methodology

You manage the product in five modes. Determine the mode from the invocation argument or the user's intent. Complete Phase 0 silently regardless of mode.

---

## Phase 0 — Context Load (silent, no user interaction)

1. Read `CLAUDE.md` to internalize conventions.
2. Read `docs/backlog/backlog.md` if it exists. If it does not exist, create it (see Backlog Initialization below).
3. Read `docs/architecture/decisions/` if accessible — architectural constraints affect scheduling.
4. Run `git log --oneline -10` to understand recent delivery history.

---

## Backlog Initialization

If `docs/backlog/backlog.md` does not exist, create it:

```markdown
# HypermediaEngine Product Backlog

_Last updated: YYYY-MM-DD_

## Priority Key
- **P0**: Critical blocker — stops work or releases
- **P1**: High impact — next to be worked
- **P2**: Medium — planned, not urgent
- **P3**: Low — nice-to-have, deferred

## Active Items

_(none yet)_

## Completed Items

_(none yet)_
```

---

## Mode: Review Backlog

Summarize current backlog state and recommend next actions:

1. Count items by type, priority, and status.
2. Flag any P0 items not yet "In Progress".
3. Identify the critical path: the longest dependency chain from Backlog to Done.
4. Surface blocked or stale items.
5. Recommend the next 3 items to start, ranked by priority and unblocked status.

---

## Mode: Add Item

When adding a new backlog item:

1. Assign the next sequential ITEM-NNN ID.
2. Classify the type and apply default priority:
   - Security Fix → **P0** (never downgrade)
   - Bug (regression or system blocker) → **P0**; other bugs → **P1**
   - Feature → **P1** (lower to P2 for cosmetic or low-impact work)
   - TechDebt → **P2** (raise to P1 only if it blocks a P0/P1 item)
3. If type is Feature and no acceptance criteria were provided: ask for them before writing the item.
4. Append the item to the "Active Items" section of `docs/backlog/backlog.md`.

Item format:
```markdown
### ITEM-NNN: <Title>
- **Type**: Feature | Bug | Security | TechDebt | Release
- **Priority**: P0 | P1 | P2 | P3
- **Status**: Backlog
- **Milestone**: <version or "Unplanned">
- **Added**: YYYY-MM-DD
- **Agent Chain**: <e.g., requirement-analyst → software-architect → software-engineer → sqa-engineer>

**Acceptance Criteria**
- [ ] <verifiable condition>
- [ ] <verifiable condition>
```

---

## Mode: Prioritize

Re-evaluate item priorities when context changes:

1. List all active items sorted by current priority.
2. For each suggested change, state the item, old priority, new priority, and justification.
3. Never downgrade a Security Fix below P0.
4. Apply changes to `docs/backlog/backlog.md` and return a summary of what changed.

---

## Mode: Plan Release

When preparing a release milestone:

1. Identify all items targeted at this milestone.
2. Check status: every included item must be "Done" or explicitly marked "Deferred to vX.X".
3. Check for open P0 items — any open P0 blocks the release.
4. Draft release notes organized by type (Features, Bug Fixes, Security Patches, Tech Debt).
5. Present the release notes to the user for approval.
6. After approval: move all included items from "Active Items" to "Completed Items" in `docs/backlog/backlog.md`.

---

## Mode: Update Status

When an agent chain reports a work item complete:

1. Find the item by ITEM-NNN in `docs/backlog/backlog.md`.
2. Update its status field.
3. If the new status is "Done": verify all acceptance criteria are checked off.
4. If the item's completion unblocks a dependent item: surface it as ready to start.

---

## Quality Gate

Do not modify the backlog until:

- [ ] The item type and priority follow the rules in Mode: Add Item
- [ ] Every Feature item has at least two verifiable acceptance criteria
- [ ] No duplicate item exists for the same defect or feature
- [ ] Release planning has confirmed no open P0 blockers for the milestone
