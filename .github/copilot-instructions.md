# Copilot Instructions

## Purpose

This file defines repository-specific operating rules for GitHub Copilot in HypermediaEngine.

> Agent-related instructions (routing, anti-hallucination, memory, file ownership, multi-platform portability) live in [AGENTS.md](../AGENTS.md).

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
- Exception: if a change is non-functional and only modifies agent artifacts (planning, agents, skills, hooks, prompts/commands, rules/instructions), you may skip both gates.
- If any runtime code, test code, runtime configuration, or build logic changes, run both gates.
