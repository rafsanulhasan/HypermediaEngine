---
description: "Creates, updates, and lists agent definitions, skill files, and command files for the HypermediaEngine multi-agent system. Invoked by the skill-manager agent. Modes: list, create-agent, update-agent, create-skill, update-skill."
agent: "agent"
argument-hint: "Mode: list | create-skill <name> | update-skill <name> | create-agent <name> | update-agent <name>"
---

This skill creates and maintains the multi-agent system's component files. Every write validates frontmatter schema and ensures all new skills include a Phase 0 context-load pattern.

Every other agent must route agent/skill creation and modification through the `agent-manager` agent rather than editing these files directly.

## Modes

### list

Enumerate all agents, skills, and command files:

1. Glob `.github/agents/*.agent.md` and `.claude/agents/*.md` — list all agents.
2. Glob `.github/skills/*/SKILL.md` and `.claude/skills/*/SKILL.md` — list all skills.
3. Glob `.github/prompts/*.prompt.md` and `.claude/commands/*.md` — list all commands/prompts.
4. Report name, description, and platform presence for each.

### create-skill `<name>`

Create a new skill on both platforms:

1. Read existing skills to understand structure patterns.
2. Create `.github/skills/<name>/SKILL.md` with:
   - YAML frontmatter: `name`, `description` (trigger words)
   - Phase 0 context-load section (mandatory)
   - Workflow phases
   - Quality gate
3. Create `.claude/skills/<name>/SKILL.md` with identical content.
4. Create `.github/prompts/<name>.prompt.md` (Copilot slash command).
5. Create `.claude/commands/<name>.md` (Claude slash command).

### update-skill `<name>`

1. Read all four files for the skill.
2. Apply the requested changes to all four files, keeping them in sync.
3. Confirm Phase 0 is still present after the update.

### create-agent / update-agent

Delegate to `agent-management` skill — do not edit agent files directly here.

## Required Frontmatter

### SKILL.md

```yaml
---
name: <skill-name>
description: "USE FOR: <trigger phrases>. DO NOT USE FOR: <anti-patterns>."
---
```

### .prompt.md (Copilot)

```yaml
---
description: "<same trigger phrases as SKILL.md>"
agent: "agent"
argument-hint: "<hint shown in chat>"
---
```

### .md (Claude command)

```yaml
---
description: "<same description as SKILL.md>"
---
```

## Quality Gate

Do not complete a skill creation or update until:

- [ ] All four files exist and are in sync (SKILL.md × 2, command × 2)
- [ ] Phase 0 context-load is present in both SKILL.md files
- [ ] `description` contains specific trigger phrases for discoverability
- [ ] No agent file was directly edited — delegate to `agent-management` skill
