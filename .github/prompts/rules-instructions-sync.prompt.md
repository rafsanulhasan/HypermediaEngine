---
description: "Detects and resolves drift between .claude/rules/*.md (Claude rules) and .github/instructions/*.instructions.md (Copilot instructions files). Ensures every rule has a matching instructions file and vice versa."
agent: "agent"
argument-hint: "Mode: audit | sync <name> | sync-all | diff <name>"
---

Audits all rule/instructions pairs for platform drift, flags orphan files, and syncs content from Claude rules to Copilot instructions files. Every `.claude/rules/<name>.md` should have a matching `.github/instructions/<name>.instructions.md`.

## Modes

- **audit** — list all rules and instructions files; flag orphans and content drift
- **sync `<name>`** — sync the named rule→instructions pair using the Claude rule as authoritative source (preserving `applyTo` frontmatter)
- **sync-all** — sync all non-deprecated rule/instructions pairs
- **diff `<name>`** — show a structured diff between `.claude/rules/<name>.md` and `.github/instructions/<name>.instructions.md`

## Expected Workflow

1. Load `.claude/CLAUDE.md`, `AGENTS.md`, and persistent memory (agent-manager).
2. Glob both rules and instructions directories to build the inventory.
3. For `audit`: normalise names, cross-reference all pairs, classify orphans and drift, emit report table.
4. For `sync <name>`: read Claude rule; derive Copilot instructions content (preserve existing `applyTo`); write instructions file.
5. For `sync-all`: iterate all non-deprecated Claude rules, sync each pair, summarise results.
6. For `diff <name>`: compare frontmatter and body section headings, output diff table.

## Safety Defaults

- Claude rule is always the authoritative source during sync.
- Copilot-specific `applyTo` frontmatter is preserved and never overwritten.
- Deprecated rules are skipped during `sync-all` (reported separately).
- No rule or instructions file is ever deleted.
