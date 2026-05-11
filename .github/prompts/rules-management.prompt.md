---
description: "Create, modify, delete, or sync Claude rules files and GitHub Copilot instruction files across both platforms using a discovery-first, non-destructive workflow. Invoked by agent-manager for all rules/instructions lifecycle operations."
agent: "agent"
argument-hint: "Operation: create <name> | modify <name> | delete <name> | sync <name> - platform: claude | copilot | both"
---

This skill creates and maintains Claude rules files (`.claude/rules/*.md`) and GitHub Copilot instruction files (`.github/instructions/*.instructions.md`). It keeps both platforms in sync and defaults to non-destructive operations.

Every other agent must route rule and instruction creation, modification, and deletion through the `agent-manager` agent rather than editing these files directly.

## Modes

### `create <name>`

Create a new rule and instruction pair for a named rule set:

1. Validate `<name>` is kebab-case; fail if both files already exist.
2. Write `.claude/rules/<name>.md` with proper frontmatter and rule content.
3. Write `.github/instructions/<name>.instructions.md` with proper frontmatter and matching instruction content.
4. Report paths written and any warnings.

### `modify <name>`

Update an existing rule/instruction pair:

1. Discover and read both existing files.
2. Apply requested changes to frontmatter and body sections.
3. Keep both files in sync unless a platform-specific scope is given.
4. Preserve all unrelated content.

### `delete <name>`

Remove or archive a rule/instruction pair:

1. Default to **soft delete**: rename to `<name>.md.deprecated` / `<name>.instructions.md.deprecated`.
2. Hard delete only with explicit flag and user confirmation.
3. Report what was archived or removed.

### `sync <name>`

Reconcile drift between the Claude rule and Copilot instruction:

1. Diff both files.
2. Identify authoritative source.
3. Update the non-authoritative file to match while preserving platform-specific frontmatter.

## Expected Workflow

1. Discover existing files before making any change.
2. Classify operation (`create`, `modify`, `delete`, `sync`) and platform scope.
3. Apply minimal, reversible edits; preserve unrelated content.
4. Validate frontmatter schema and pairing integrity.
5. Report summary, any drift found, and recommended next steps.

## Safety Defaults

- Prefer non-destructive changes.
- For delete operations, soft-delete by default; request explicit confirmation before hard delete.
- Both platform files are written/updated together unless scope is restricted.
- Never read or modify `.env` or sensitive configuration files.
