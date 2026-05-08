---
description: "Creates, updates, syncs, and deprecates agent definitions for both Claude Code (.claude/agents/*.md) and GitHub Copilot/VS Code (.github/agents/*.agent.md). Invoke when creating a new agent, updating an existing one, or syncing platform drift."
agent: "agent"
argument-hint: "Mode and agent name: create <name> | update <name> | sync <name> | deprecate <name>"
---

This skill manages agent definitions across both platforms. Every agent definition must exist in both:

- `.claude/agents/<name>.md` — Claude Code platform
- `.github/agents/<name>.agent.md` — GitHub Copilot / VS Code platform

## Modes

### create `<agent-name>`

1. Read existing agents in `.claude/agents/` and `.github/agents/` to understand current naming and structure patterns.
2. Read `.github/copilot-instructions.md` and `CLAUDE.md` for project-wide conventions that all agents must follow.
3. Determine the agent's role, trigger words, and capabilities based on the user's description.
4. Create `.claude/agents/<name>.md` with:
   - YAML frontmatter: `name`, `description` (trigger words and capabilities), optionally `tools` and `model`
   - Phases 0–N covering the agent's workflow
   - Quality gate
5. Create `.github/agents/<name>.agent.md` mirroring the same behavior, adapted for Copilot tool aliases.
6. Add the agent name to the shared agent name mapping in both `CLAUDE.md` and `AGENTS.md`.

### update `<agent-name>`

1. Read the current `.claude/agents/<name>.md` to understand what needs changing.
2. Apply the requested changes.
3. Mirror all behavioral changes to `.github/agents/<name>.agent.md`.
4. Confirm both files are in sync after the update.

### sync `<agent-name>`

When one platform file is ahead of the other:

1. Identify which file is the source of truth (usually the Claude version for behavior).
2. Read both files and diff them.
3. Apply missing changes to the stale file.
4. Confirm both files describe identical behavior after sync.

### deprecate `<agent-name>`

1. Add `**DEPRECATED**` at the top of the description in both files.
2. Add a note stating which agent replaced it and when.
3. Do NOT delete the files.

## Agent File Structure

### `.claude/agents/<name>.md`

```markdown
---
name: <agent-name>
description: "Use for <purpose>. Trigger words: <list>."
---

<Agent behavior phases>
```

### `.github/agents/<name>.agent.md`

```markdown
---
description: "<same description as Claude file>"
tools: [<tool list>]
---

<Same agent behavior, using Copilot tool names>
```

## Quality Gate

Do not complete an agent creation or update until:

- [ ] Both `.claude/agents/<name>.md` and `.github/agents/<name>.agent.md` exist
- [ ] Both files describe identical behavior
- [ ] The agent name is in `AGENTS.md` and `CLAUDE.md`
- [ ] The `description` field contains specific trigger words so agents and users can discover it
