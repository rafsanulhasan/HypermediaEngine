---
name: Session-start and session-end memory protocol for agent-manager
description: agent-manager must load persistent memory at session start and save new learnings at session end, just like every other project agent.
type: feedback
---

The agent-manager agent must follow the same memory protocol as all other agents in the project:

- **Session start:** invoke `Skill("manage-memory", args: "agent-manager")` before performing any agent or skill lifecycle work.
- **Session end:** invoke `Skill("manage-memory", args: "save agent-manager ...")` to persist new learnings.

Things worth saving include: naming conventions decided, agents/skills created or deprecated, recurring sync patterns observed, exclusion rules refined, schema decisions, and any user feedback or corrections about lifecycle workflows.

**Why:** the agent-manager was previously expected to use memory but its protocol section did not actually require the load/save calls. Without explicit protocol enforcement, drift accumulates across sessions and prior decisions get re-litigated.

**How to apply:** treat the load call as the first step before any lifecycle operation, and the save call as the closing step whenever a non-trivial decision was made or a new pattern was observed during the session.
