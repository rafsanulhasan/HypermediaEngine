# Copilot Instructions

## Purpose

This file defines repository-specific operating rules for GitHub Copilot in HypermediaEngine.

## Core Build And Test Commands

- `dotnet build` to build the solution or project
- `dotnet run` to run the project
- `dotnet test` to run the test suite
- `dotnet stryker` to run mutation testing

## Architectural Priorities

- Middleware-centric request pipeline
- Dependency Injection as a default integration mechanism
- Endpoint Filters and Result Filters for API cross-cutting behavior

## Coding Conventions

- Keep C# extension blocks (extension members) in `QueryableHelpers`; do not convert them to static extension methods.
- Prefer explicit type declarations with target-typed `new` and collection expressions where appropriate.
- Allowed exceptions:
	- `Stream stream = new FileStream(...)`
	- `IEnumerable<T> items = new List<T>()`
- Prefer async disposal over sync disposal (`await using` when supported).
- API return shape must be `{ data, error }`.
- Never expose stack traces to clients.
- Use the project logger module, never `console.log`.

## Quality Gates

- After each implemented feature or fix: run `dotnet test`.
- After tests pass: run `dotnet stryker`.

## Agent Execution Protocol

### 1. Routing

- For non-trivial requests, start with `Skill("triage")` before any specialist skill or agent.
- Skip triage only for:
	- simple factual questions
	- follow-up work within an already triaged workflow
- Triage must output a decomposition, dependency map, and a confirmed execution plan.

### 2. Memory

- At session start: `Skill("manage-memory", args: "<agent-name>")`
- At session end: `Skill("manage-memory", args: "save <agent-name> ...")`
- For non-routine memory operations (prune/audit/refresh): route through `Agent("skill-manager", prompt: "prune/audit/refresh <agent-name>")`

### 3. Skill And Agent File Ownership

- All agent, skill, and command file creation/modification must go through `Agent("skill-manager", prompt: "...")`.
- Do not directly edit:
	- `.claude/agents/*.md`
	- `.claude/commands/*.md`
	- `agents/skills/*/SKILL.md`
	- `.agents/skills/*/SKILL.md`
- When creating a new skill, always create both:
	- `.agents/skills/<name>/SKILL.md`
	- `.claude/commands/<name>.md`

## Multi-Platform Portability

This repository maintains parallel behavior across Claude Code and Copilot/VS Code agent systems.

### Platform Structure

- `.claude/agents/*.md` and `.claude/commands/*.md`: Claude-first definitions/workflows
- `.claude/skills/*/SKILL.md`: Claude skill discovery
- `.github/agents/*.agent.md`: Copilot/VS Code agent definitions
- `.agents/skills/*/SKILL.md`: shared Copilot/VS Code skill discovery

### Shared Agent Names

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

### Synchronization Rules

- Agent behavior updates must be mirrored in both:
	- `.claude/agents/<name>.md`
	- `.github/agents/<name>.agent.md`
- Skill behavior updates must be mirrored in both:
	- `.claude/skills/<skill>/SKILL.md`
	- `.agents/skills/<skill>/SKILL.md`