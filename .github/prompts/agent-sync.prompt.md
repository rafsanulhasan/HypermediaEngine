---
description: "Detects and resolves platform drift between .claude/agents/*.md (Claude Code) and .github/agents/*.agent.md (Copilot/VS Code). Produces a drift report and applies corrective edits."
agent: "agent"
argument-hint: "Mode: audit | sync <name> | sync-all | diff <name>"
---

Audits the full agent roster for platform drift and applies corrective syncs. Applies the Claude-only Tool Exclusion List automatically whenever syncing — `Bash`, `Glob`, `Grep`, `Read`, `TodoWrite`, `WebFetch`, `WebSearch`, `PushNotification`, and `ToolSearch` are never copied to `.github/agents/`.

## Modes

- **audit** — scan all agents on both platforms; report missing files, frontmatter differences, and tools array divergence
- **sync `<name>`** — sync a named agent from `.claude/agents/<name>.md` → `.github/agents/<name>.agent.md` applying tool exclusion rules
- **sync-all** — run sync for every non-deprecated agent in the roster
- **diff `<name>`** — show a structured diff between the Claude and Copilot versions of one agent

## Expected Workflow

1. Load `.claude/CLAUDE.md`, `AGENTS.md`, and persistent memory (agent-manager).
2. Glob both agent directories to build the roster index.
3. For `audit`: cross-reference all agents, classify drift, emit report table.
4. For `sync <name>`: read Claude definition, apply tool exclusion list, write Copilot counterpart.
5. For `sync-all`: iterate roster, sync each non-deprecated agent, summarise results.
6. For `diff <name>`: compare frontmatter field-by-field and body section headings, output diff table.

## Safety Defaults

- Deprecated agents are skipped during `sync-all` (reported separately).
- No agent file is ever deleted.
- Tool exclusion is non-negotiable and always applied.
