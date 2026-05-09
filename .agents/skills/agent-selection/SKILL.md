---
name: agent-selection
description: Expert agent selection skill. Use PROACTIVELY to select the most suitable agent for a given task, based on the task requirements and agent capabilities. Also use this skill to orchestrate multi-agent workflows when a task requires multiple skills or expertise.
---

# agent-selection

## Workflow

```
requirement-analyst → software-architect + system-engineer → software-engineer → (sqa-engineer + documentation-writer) → code-reviewer
```

Use `triage-agent` to orchestrate multi-agent workflows. After `software-engineer` completes any implementation work (feature, bug fix, or refactor), `sqa-engineer` and `documentation-writer` start in parallel.

## Agent Reference

- **requirement-analyst** — Analyzes requirements. Invoke before any feature work begins.

- **software-architect** — Designs high-level system architecture. Invoke after `requirement-analyst` completes, and after `software-engineer` finishes to validate architectural integrity.

- **system-engineer** — Designs low-level structure: SOLID, design patterns, monads, discriminated unions. Collaborates with `software-architect` to translate requirements into concrete design. Also validates implementation after `software-engineer` finishes.

- **software-engineer** — Implements features, fixes bugs, refactors. Invoke after architecture and design are defined.

- **code-reviewer** — Reviews implementation and test cases after `software-engineer`, `sqa-engineer`, and `documentation-writer` complete their work.

- **sqa-engineer** — Designs and writes test cases, validates tests after `software-engineer` implements features, fixes bugs, and refactors code. Runs in parallel with `documentation-writer`.

- **triage-agent** — Orchestrates and manages multi-agent workflows. Use when a task spans multiple agents or requires coordinated parallel execution.

- **product-manager** — Owns the product backlog, prioritization, and release planning. Consult before starting Feature or TechDebt work. Invoke for release planning and backlog health reviews.

- **documentation-writer** — Writes and maintains documentation, updates README, and updates other documentation such as release notes. Invoked after `software-engineer` completes any implementation work. Runs in parallel with `sqa-engineer`.

- **agent-manager** — Manages agent definitions and lifecycle. Use to create, update, or deprecate agents as the system evolves.
