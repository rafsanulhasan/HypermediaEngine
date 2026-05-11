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
- `agent-manager`
- `documentation-writer`
- `research-assistant`

## Agent Protocols

### Routing

- **All user prompts must be routed through the triage-agent first** — invoke `Agent("triage-agent", prompt: "...")` (Claude) or `@triage-agent` (Copilot) before any specialist agent or skill, unless the user is asking a simple factual question or a follow-up within an already-triaged workflow.
- The triage-agent classifies the request, decomposes multi-step tasks, maps dependencies, and produces a confirmed execution plan before routing to specialist agents.
- Do not skip triage to save time — incorrect routing wastes more time than triage costs.

### Anti-Hallucination

- No agent may respond with hallucinated, vague, or ambiguous content.
- When unsure about a factual claim, library/API behavior, or non-trivial codebase fact, invoke one or more `research-assistant` subagents **in parallel** (single message, multiple Agent calls) — one focused question per spawn.
- If research is inconclusive or the ambiguity is about user intent, **ask the user** a targeted clarifying question rather than guessing.
- Prefer "I don't know — let me verify" over a confident guess.

### Memory

- Session start: `Skill("manage-memory", args: "<agent-name>")` to load persistent memory
- Session end: `Skill("manage-memory", args: "save <agent-name> ...")` to save new learnings
- For prune, audit, or refresh: `Agent("agent-manager", prompt: "prune/audit/refresh <agent-name>")`

### File Ownership By Agent

**Agent definitions and skill files** are owned by `agent-manager`. Don't create or update or delete any files directly from the following paths:

- `.claude/**`
- `.github/**`
- `.agents/**`
