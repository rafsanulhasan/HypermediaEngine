---
name: Capability Gap Handling added to triage-agent
description: triage-agent on both platforms now has a "Capability Gap Handling" section requiring it to delegate to agent-manager to create missing skill/hook/command/MCP capabilities before routing.
type: project
---

triage-agent (both `.claude/agents/triage-agent.md` and `.github/agents/triage-agent.agent.md`) carries a "Capability Gap Handling" section between Behavioral Principles and Skills/Preferred Skills. It mandates: identify the missing capability, delegate to agent-manager to create it (attach to existing agent or create new agent), then route the original request.

**Why:** Prevents triage from forcing requests onto ill-fitting agents when a true capability gap exists. Routes capability-creation requests back to agent-manager (the file owner per CLAUDE.md) instead of having specialists improvise.

**How to apply:** When updating triage-agent behavior in future, preserve this section. When auditing other agents for similar gap-handling logic, this is the canonical pattern.
