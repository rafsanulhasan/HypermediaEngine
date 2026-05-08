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
- For prune, audit, or refresh: `Agent("skill-manager", prompt: "prune/audit/refresh <agent-name>")`

### Skill & Agent Updates

- All agent definitions, skill files, and command files must be created or modified through `Agent("skill-manager", prompt: "...")`
- No agent edits `.claude/agents/*.md`, `agents/skills/*/SKILL.md`, or `.claude/commands/*.md` directly
