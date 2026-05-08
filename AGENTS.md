# HypermediaEngine Multi-Platform Agent Index

This repository is configured for **Claude Code**, **GitHub Copilot CLI**, and **VS Code custom agents**.

## Platform Layout

- `.claude/agents/*.md` and `.claude/commands/*.md` are the Claude-first agent definitions and command workflows.
- `.claude/skills/*/SKILL.md` is the Claude Code skill discovery layer.
- `.github/agents/*.agent.md` is the Copilot/VS Code custom agent layer.
- `.agents/skills/*/SKILL.md` is the shared skill layer for Copilot/VS Code skill discovery.

## Agent Name Mapping

The same role names are used across platforms to keep delegation predictable:

- `triage-agent`
- `requirement-analyst`
- `software-architect`
- `system-engineer`
- `software-engineer`
- `sqa-engineer`
- `code-reviewer`
- `product-manager`
- `skill-manager`
- `agent-manager`

## Portability Rule

When updating agent behavior, update both:

1. `.claude/agents/<name>.md` (Claude behavior and examples)
2. `.github/agents/<name>.agent.md` (Copilot/VS Code behavior and tool aliases)

When updating skill behavior, update:

1. `.claude/skills/<skill>/SKILL.md` (discoverable copy for Claude Code)
2. `.agents/skills/<skill>/SKILL.md` (discoverable copy for Copilot/VS Code)
