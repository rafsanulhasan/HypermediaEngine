---
description: "Detects and resolves drift between .claude/skills/<name>/SKILL.md and .github/skills/<name>/SKILL.md. Also validates that every skill has a matching command (.claude/commands/<name>.md) and prompt (.github/prompts/<name>.prompt.md)."
agent: "agent"
argument-hint: "Mode: audit | sync <name> | sync-all | diff <name>"
---

Audits the full skills inventory across all four required files per skill and syncs SKILL.md content from `.claude/skills/` to `.github/skills/`.

## Modes

- **audit** — scan all skill directories; report missing counterparts across all four required files and content drift between SKILL.md pairs
- **sync `<name>`** — sync `.claude/skills/<name>/SKILL.md` → `.github/skills/<name>/SKILL.md`
- **sync-all** — sync all skills found in the Claude skills directory
- **diff `<name>`** — show a structured diff for one skill (SKILL.md frontmatter, Phase 0 presence, companion file status)

## Expected Workflow

1. Load `.claude/CLAUDE.md`, `AGENTS.md`, and persistent memory (agent-manager).
2. Glob all four file surfaces (`.claude/skills/`, `.github/skills/`, commands, prompts) to build the full inventory.
3. For `audit`: classify each skill as complete / missing-file / drifted; emit report table with all four columns.
4. For `sync <name>`: read Claude SKILL.md, create `.github/skills/<name>/` if needed, write identical content.
5. For `sync-all`: iterate all non-deprecated Claude skills, sync each, summarise results.
6. For `diff <name>`: compare frontmatter fields, Phase 0 section, body headings, and companion file presence.

## Safety Defaults

- `.claude/skills/<name>/SKILL.md` is always the authoritative source.
- Deprecated skills are skipped during `sync-all` (reported separately).
- Phase 0 presence is validated on every write.
- No skill file is ever deleted.
