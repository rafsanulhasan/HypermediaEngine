---
description: "Detects and resolves drift between .claude/commands/*.md (Claude commands) and .github/prompts/*.prompt.md (Copilot prompt files). Ensures every command has a matching prompt and vice versa."
agent: "agent"
argument-hint: "Mode: audit | sync <name> | sync-all | diff <name>"
---

Audits all command/prompt pairs for platform drift, flags orphan files, and syncs content from Claude commands to Copilot prompts. Every `.claude/commands/<name>.md` must have a matching `.github/prompts/<name>.prompt.md`.

## Modes

- **audit** — list all commands and prompts; flag orphans (command without prompt or vice versa) and content drift
- **sync `<name>`** — sync the named command→prompt pair using the Claude command as authoritative source
- **sync-all** — sync every command/prompt pair in the repo
- **diff `<name>`** — show a structured diff between `.claude/commands/<name>.md` and `.github/prompts/<name>.prompt.md`

## Expected Workflow

1. Load `.claude/CLAUDE.md`, `AGENTS.md`, and persistent memory (agent-manager).
2. Glob both commands and prompts directories to build the inventory.
3. For `audit`: cross-reference all pairs, classify orphans and drift, emit report table.
4. For `sync <name>`: read Claude command `description`, derive Copilot prompt content, write prompt file.
5. For `sync-all`: iterate all commands, sync each pair, summarise results.
6. For `diff <name>`: compare frontmatter and body section headings, output diff table.

## Safety Defaults

- Claude command is always the authoritative source during sync.
- Copilot prompt files always retain `agent: "agent"` frontmatter.
- No command or prompt file is ever deleted.
