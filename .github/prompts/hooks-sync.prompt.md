---
description: "Detects and resolves drift between Claude Code hooks (.claude/hooks/*.ps1 + .claude/settings.json) and GitHub Copilot hooks (.github/hooks/*.ps1 + .github/hooks/*.json). Ensures semantic parity across both hook surfaces."
agent: "agent"
argument-hint: "Mode: audit | sync <name> | sync-all | diff <name>"
---

Audits the full hook inventory for platform drift and applies corrective syncs across `.claude/settings.json`, Claude hook scripts, and Copilot hook scripts and metadata files.

## Modes

- **audit** — inventory all hooks on both platforms; flag unregistered scripts, missing metadata, and semantic drift
- **sync `<name>`** — sync a named hook from Claude → Copilot (or Copilot → Claude when only the Copilot hook exists)
- **sync-all** — sync all hooks detected across both platforms
- **diff `<name>`** — show a structured diff for one hook name (script content, event type, registration status)

## Expected Workflow

1. Load `.claude/CLAUDE.md`, `AGENTS.md`, `settings.json`, and persistent memory (agent-manager).
2. Glob all hook scripts and metadata to build the inventory.
3. For `audit`: cross-reference hooks, classify unregistered/missing/drifted entries, emit report table.
4. For `sync <name>`: read Claude hook + `settings.json` entry; write Copilot `.ps1` and `.json` (or reverse).
5. For `sync-all`: iterate all hooks from both registries, sync each, summarise results.
6. For `diff <name>`: compare script content, event types, and registration status.

## Safety Defaults

- No hook file is ever deleted.
- `settings.json` modifications are additive only (never remove existing entries unless explicitly deleting a hook).
- Sync defaults Claude → Copilot; reverses only when Claude hook is absent.
