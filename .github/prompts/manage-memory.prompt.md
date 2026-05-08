---
description: "Manages persistent file-based memory for agents in .github/agent-memory/<agent-name>/. Supports load, save, refresh, prune, and audit operations. Agents call this for routine load/save; route prune/audit/refresh through skill-manager."
agent: "agent"
argument-hint: "Operation and agent name: load <agent> | save <agent> | refresh <agent> | prune <agent> | audit"
---

All agents use this skill for memory operations:

- **load `<agent-name>`** — load all memories at session start
- **save `<agent-name>`** — save new learnings at session end
- **refresh `<agent-name>`** — force-reload the memory index
- **prune `<agent-name>`** — remove stale/duplicate entries (requires confirmation)
- **audit** — survey memory health across all agents

For prune, audit, and refresh operations, prefer invoking via the `skill-manager` agent so changes are tracked.

## Memory Location

Agent memory files are stored in `.github/agent-memory/<agent-name>/`. Each agent has:

- `index.md` — the memory index listing all memory file references
- Individual memory files for topics (e.g., `patterns.md`, `decisions.md`, `lessons.md`)

## Load Operation

1. Check if `.github/agent-memory/<agent-name>/index.md` exists.
2. If it exists, read the index to discover all memory files.
3. Read each referenced memory file.
4. Summarize what was loaded (silent, no user interaction needed).

## Save Operation

1. Identify new learnings from the current session: routing patterns, convention violations found, architectural decisions made, test patterns discovered.
2. Check if an appropriate memory file already exists for each learning.
3. If it exists, append to it. If not, create a new file and add it to the index.
4. Update `index.md` with any new file references.
5. Keep entries short and actionable — bullet points, not paragraphs.

## Prune Operation (requires user confirmation)

1. Read all memory files for the agent.
2. Identify entries that are:
   - Outdated (contradict current code state)
   - Duplicated (same information in two files)
   - Too vague to be actionable
3. Present the proposed deletions to the user before making any changes.
4. Apply only confirmed deletions.

## Audit Operation

1. Enumerate all agents with memory directories.
2. For each agent, report: number of files, approximate size, last modified date.
3. Flag agents with no memory (may need initialization) or very large memory (may need pruning).
