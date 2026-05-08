---
name: agent-selection
description: Expert agent selection skill. Use PROACTIVELY to select the most suitable agent for a given task, based on the task requirements and agent capabilities. Also use this skill to orchestrate multi-agent workflows when a task requires multiple skills or expertise.
model: claude-haiku-4-5-20251001
tools: Read, Grep, Glob, agent
---

# Agents

## Workflow

```
requirement-analyst → software-architect + system-engineer → software-engineer → code-reviewer → software-tester
```

Use `triage-agent` to orchestrate multi-agent workflows.

## Agent Reference

- **requirement-analyst** — Analyzes requirements. Invoke before any feature work begins.

- **software-architect** — Designs high-level system architecture. Invoke after `requirement-analyst` completes, and after `software-engineer` finishes to validate architectural integrity.

- **system-engineer** — Designs low-level structure: SOLID, design patterns, monads, discriminated unions. Collaborates with `software-architect` to translate requirements into concrete design. Also validates implementation after `software-engineer` finishes.

- **senior-system-engineer** — Same focus as `system-engineer` but with extended tooling (Bash, Write, mcp). Use when the design task requires file edits or shell execution.

- **software-engineer** — Implements features, fixes bugs, refactors. Invoke after architecture and design are defined.

- **code-reviewer** — Reviews implementation after `software-engineer` completes.

- **software-tester** — Writes and validates tests after `code-reviewer` approves.

- **triage-agent** — Orchestrates and manages multi-agent workflows. Use when a task spans multiple agents or requires coordinated parallel execution.

- **product-manager** — Owns the product backlog, prioritization, and release planning. Consult before starting Feature or TechDebt work. Invoke for release planning and backlog health reviews.
