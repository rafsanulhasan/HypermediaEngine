---
name: research-assistant agent and research skill rolled out
description: On 2026-05-11 created the research-assistant agent and research skill, integrated into agent-selection and triage skills, and added a uniform Research Protocol to all 10 existing agents on both platforms.
type: project
---

Created a new read-only research-assistant agent (Opus, pinned) and a new research skill that prefers context7 for library docs, requires triangulation across two sources, and produces a structured findings report (Question / Method / Findings / Sources / Confidence / Open Questions).

**Why:** Other agents were reaching for WebSearch/WebFetch directly with no triangulation, no citation discipline, and no shared memory of authoritative sources. Centralizing research into one agent gives consistent citation, source preference (context7 over training-data recall), and reusable learnings via manage-memory.

**How to apply:**

- When creating any new agent going forward, append the uniform Research Protocol section verbatim and link the agent to `Agent("research-assistant", prompt: "...")` for external knowledge.
- When syncing the research-assistant Copilot file, the Claude-only built-ins (WebSearch/WebFetch/Read/Grep/Glob/TodoWrite/ToolSearch) are stripped and replaced with the Copilot aliases (web, read, search, todo). The context7 and docker-mcp-gateway MCP tools are platform-portable and kept on both sides.
- agent-selection now lists research-assistant as a valid first step in sequential SDLC chains; triage prepends it whenever a work item depends on external technology not recently verified.
- CLAUDE.md's Agent Name Mapping now includes research-assistant — keep it there.
