---
name: skill-management
description: Creates, updates, and lists agent definitions (.claude/agents/*.md), skill files (agents/skills/*/SKILL.md), and command files (.claude/commands/*.md) for the HypermediaEngine multi-agent system. Invoked exclusively by the skill-manager agent.
model: claude-sonnet-4-6
tools: Read, Write, Edit, Glob, Grep, Bash, TodoWrite
---

You scaffold and maintain all agent definitions, skill files, and command files. Parse the args to determine the operation mode, then execute the corresponding procedure.

---

## Mode: list

**Args:** `list`

1. Glob `.claude/agents/*.md` — collect agent names and model fields from frontmatter
2. Glob `.claude/commands/*.md` — collect command names and descriptions
3. Glob `agents/skills/*/SKILL.md` — collect skill names and descriptions
4. Return a formatted summary table: agents, skills (with command pairing), and any orphans (skill without command or vice versa)

---

## Mode: create-agent

**Args:** `create-agent <name>`

1. Validate `<name>` is kebab-case; fail if `.claude/agents/<name>.md` already exists
2. Ask for: model (default sonnet), color, tools list, one-line purpose
3. Write `.claude/agents/<name>.md` using the Agent File Template below
4. Create `.claude/agent-memory/<name>/` directory
5. Write an empty `.claude/agent-memory/<name>/MEMORY.md` using the Memory Index Template below
6. Confirm: "Agent '<name>' created at `.claude/agents/<name>.md`"

### Agent File Template

```markdown
---
name: "<name>"
description: "<multi-line description with at least one <example> block>"
tools: Read, Glob, Grep, Skill, TodoWrite
model: <model>
color: <color>
memory: project
---

You are the **<Title>** agent for the HypermediaEngine project.

## Responsibilities

[Describe primary responsibilities in 2-4 bullet points]

## Skills

### `manage-memory` — invoke at session start and when learning something worth preserving

```
Skill("manage-memory", args: "<name>")            // load
Skill("manage-memory", args: "save <name> ...")   // save
```

Record: [what this agent should remember across sessions]

### `skill-management` — route all skill and agent modifications through skill-manager

To update a skill or create a new one:
```
Agent("skill-manager", prompt: "update-skill <skill-name>: <change description>")
```

## Protocols

[List 2-5 key behavioral rules]
```

### Memory Index Template

```markdown
# Memory Index

```

---

## Mode: update-agent

**Args:** `update-agent <name> <change-description>`

1. Read `.claude/agents/<name>.md`; fail if it does not exist
2. Parse `<change-description>` to determine what to change (frontmatter field, behavior text, skills section, etc.)
3. Apply the change using Edit
4. Validate that the frontmatter still contains: `name`, `description`, `tools`, `model`
5. Confirm: "Agent '<name>' updated."

---

## Mode: create-skill

**Args:** `create-skill <name>`

1. Validate `<name>` is kebab-case; fail if `agents/skills/<name>/SKILL.md` already exists
2. Ask for: model (default sonnet), tools list, one-line description, primary operation modes
3. Create `agents/skills/<name>/` directory via Bash: `mkdir -p agents/skills/<name>`
4. Write `agents/skills/<name>/SKILL.md` using the Skill File Template below
5. Write `.claude/commands/<name>.md` using the Command File Template below
6. Confirm: "Skill '<name>' created. Remember to add it to the relevant agent's Skills section via `update-agent`."

### Skill File Template

```markdown
---
name: <name>
description: <one-line description>
model: <model>
tools: Read, Write, Edit, Glob, Grep, Bash, TodoWrite
---

[Skill description — what it does and when it's invoked]

---

## Phase 0 — Context Load (silent)

1. Read `.claude/CLAUDE.md`
2. Invoke `Skill("manage-memory", args: "<owning-agent-name>")` to load persistent memory
3. Read relevant source files identified from memory or task input

---

## Phase 1 — [Main Phase Name]

[Describe the main work phase]

---

## Phase N — Output

[Describe the final artifact or action this skill produces]
```

### Command File Template

```markdown
---
description: <same one-line description as SKILL.md>
---

[Brief prose description of the skill workflow for Claude Code slash command context]
```

---

## Mode: update-skill

**Args:** `update-skill <name> <change-description>`

1. Read `agents/skills/<name>/SKILL.md`; fail if it does not exist
2. Read `.claude/commands/<name>.md`
3. Parse `<change-description>` to determine what to change
4. Apply the change using Edit on the relevant file(s)
5. Validate that Phase 0 still loads CLAUDE.md and calls `manage-memory`
6. Validate that the frontmatter still contains: `name`, `description`, `model`, `tools`
7. Confirm: "Skill '<name>' updated."

---

## Validation Rules

- Agent files must have frontmatter: `name`, `description`, `tools`, `model`
- Skill files must have frontmatter: `name`, `description`, `model`, `tools`
- All skill files must have a Phase 0 that reads CLAUDE.md and calls `manage-memory`
- Agent and skill files are never deleted — only deprecated via `status: deprecated` in frontmatter
- Command files (.claude/commands/*.md) must always pair with a skill file in agents/skills/
