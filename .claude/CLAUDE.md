# Project: HypermediaEngine

## Commands

- dotnet build : Build Project or Solution
- dotnet run : Run Project
- dotnet test : Run tests
- dotnet stryker : Run Mutation Tests

## Architecture

- Middlewares
- Dependency Injection
- Endpoint Filters / Result Filters

## Conventions

- Use Explicit Type declarations with Tartet typed new expression or collection expression. (e.g FileStream stream = new(), List<int> intList = [];)
  - Exception 1: Stream stream = new FileStream()
  - Exception 2: IEnumerable<int> intStream = new List<int>()
- Prefer Async Disposal over Sync Disposal. (e.g. await using FileStream stream = new FileStream()). 
- Return shape is always { data, error }
- Never expose stack traces to the client
- Use the logger module, not console.log

## Watch out for

- Run test cases after building every feature using `dotnet test`
- Run Mutation tests after running all tests using `dotnet stryker`

## Agent Protocols

### Routing

- **All user prompts must be routed through the triage skill first** — invoke `Skill("triage")` before any specialist agent or skill, unless the user is asking a simple factual question or a follow-up within an already-triaged workflow.
- The triage skill classifies the request, decomposes multi-step tasks, maps dependencies, and produces a confirmed execution plan before routing to specialist agents.
- Do not skip triage to save time — incorrect routing wastes more time than triage costs.

### Memory

- Session start: `Skill("manage-memory", args: "<agent-name>")` to load persistent memory
- Session end: `Skill("manage-memory", args: "save <agent-name> ...")` to save new learnings
- For prune, audit, or refresh: `Agent("agent-manager", prompt: "prune/audit/refresh <agent-name>")`

### File Ownership By Agent

**Agent definitions and skill files** are owned by `agent-manager`:
- Route all agent creation/update requests to `Agent("agent-manager", prompt: "...")`
- Route all skill/command creation/update requests to `Agent("agent-manager", prompt: "...")`
- No agent edits `.claude/agents/*.md`, `.github/agents/*.agent.md`, `.agents/skills/*/SKILL.md`, `.claude/skills/*/SKILL.md`, or `.claude/commands/*.md` directly

**Routing rule:** When a user asks to create or update an agent or a skill → `agent-manager`.

## Multi-Platform Agent Portability

This repository is configured for **Claude Code**, **GitHub Copilot CLI**, and **VS Code custom agents**.

### Platform Layout

- `.claude/agents/*.md` and `.claude/commands/*.md` are the Claude-first agent definitions and command workflows.
- `.claude/skills/*/SKILL.md` is the Claude Code skill discovery layer.
- `.github/agents/*.agent.md` is the Copilot/VS Code custom agent layer.
- `.agents/skills/*/SKILL.md` is the shared skill layer for Copilot/VS Code skill discovery.

### Agent Name Mapping

Use the same role names across platforms:

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

### Portability Rule

When updating agent behavior, update both:

1. `.claude/agents/<name>.md` (Claude behavior and examples)
2. `.github/agents/<name>.agent.md` (Copilot/VS Code behavior and tool aliases)

When updating skill behavior, update:

1. `.claude/skills/<skill>/SKILL.md` (discoverable copy for Claude Code)
2. `.agents/skills/<skill>/SKILL.md` (discoverable copy for Copilot/VS Code)
