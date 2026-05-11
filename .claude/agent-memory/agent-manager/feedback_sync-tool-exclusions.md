---
name: Claude-only tool exclusion when syncing to Copilot
description: When projecting Claude agent tools: frontmatter into a Copilot agent file, strip Claude Code CLI built-ins; only MCP tools and Copilot-native aliases survive.
type: feedback
---

When syncing or creating a `.github/agents/*.agent.md` file from a `.claude/agents/*.md` source, the following Claude Code-specific built-in tools MUST NOT be propagated into the Copilot `tools:` array:

- `Bash`, `Glob`, `Grep`, `Read`, `TodoWrite`, `WebFetch`, `WebSearch`, `PushNotification`, `ToolSearch`

What survives the projection:

- MCP tools (e.g. `mcp__docker-mcp-gateway__search`, `microsoft_docs_search`) — kept verbatim, they are platform-portable.
- Copilot-native aliases (`read`, `edit`, `search`, `todo`, `execute`, `agent`, `vscode/memory`, `vscode/askQuestions`, `vscode/toolSearch`).

When a Claude built-in has a 1:1 Copilot equivalent, translate it during sync (e.g. `Read` → `read`, `Bash` → `execute`, `Glob`/`Grep` → `search`, `TodoWrite` → `todo`).

**Why:** these built-ins are Claude Code CLI primitives that do not exist in the GitHub Copilot / VS Code custom agent runtime. Propagating them yields invalid Copilot agent configurations that fail silently or produce unusable agents.

**How to apply:** every `create-agent`, `update-agent`, and `sync-agent` operation in the `agent-management` skill must run the translation/exclusion procedure before writing the `tools:` frontmatter to a Copilot file. Validation must reject Copilot writes that still contain any excluded name.
