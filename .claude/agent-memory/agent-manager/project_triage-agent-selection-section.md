---
name: triage-agent uses agent-selection skill per-batch with four orchestration modes
description: The triage-agent's "Using the agent-selection skill" section codifies the four-mode contract and per-cycle batched invocation; keep both platform files in sync.
type: project
---

The triage-agent's behavior contract for `agent-selection` is: invoke once per triage cycle (not per work item), pass the whole decomposed batch, and treat the returned (orchestration-mode, agent-chain) tuple per item as binding.

The four orchestration modes are:

1. Direct single-agent delegation
2. Parallel independent subagents
3. Sequential SDLC Agent Teams
4. Full SDLC traversal (with product-manager decomposition)

**Why:** the `agent-selection` skill was updated to return these four modes and per-cycle batch reasoning. The triage-agent previously called the skill once per work item, which fragmented cross-item dependency reasoning. PM consultation for Feature/TechDebt items must happen before `agent-selection` so the PM's priority is part of the batch context.

**How to apply:** when modifying triage-agent on either platform, preserve the "Using the agent-selection skill" section verbatim in behavior — only Copilot tool aliases (`agent`, `todo`, `read`, `search`) may differ from the Claude version (`Agent`, `TodoWrite`, etc.). Never reintroduce per-item `agent-selection` calls.
