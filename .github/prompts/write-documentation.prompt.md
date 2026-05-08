---
description: "Structured documentation writing workflow for HypermediaEngine. Writes or updates README.md files at the repo root and in each project directory (src/, tests/, samples/, etc.) based on current implementation, ADRs, and architecture decisions."
agent: "agent"
argument-hint: "Scope: 'all' for full repo, or path to specific directory (e.g. src/EntityTagCaching)"
---

Invoke this prompt to write or update README.md files throughout the HypermediaEngine repository.

The workflow:

1. **Discover scope** — determine which README.md files need to be created or updated based on the argument:
   - If argument is `all` or omitted: scan the entire repository for directories that have no README.md or have stale ones.
   - If argument is a specific path: scope to that directory and its immediate children.

2. **Gather context** — for each target directory:
   - Read all source files to understand what the component does.
   - Read any ADRs in `docs/architecture/decisions/` that relate to the component.
   - Read `docs/backlog/backlog.md` to understand what is in progress and what is planned.
   - Read the root `CLAUDE.md` for project-level conventions that belong in the root README.

3. **Draft documentation** using this consistent structure for each README.md:
   - **Overview** — one paragraph: what this project/component does and why it exists
   - **Architecture** — how it fits into the overall HypermediaEngine pipeline (middleware, filters, DI)
   - **Getting Started** — prerequisites, how to build (`dotnet build`), how to run
   - **Usage** — code examples for the most common use cases
   - **Configuration** — DI registration, extension methods, options/settings
   - **Contributing** — how to run tests (`dotnet test`) and mutation tests (`dotnet stryker`)

4. **Validate coverage** — confirm that:
   - All public APIs mentioned in source files appear in the README
   - All middleware pipeline stages are documented in order
   - All extension methods for DI registration are shown with examples

5. **Write the files** — create or overwrite each README.md with the drafted content.

After completing all writes, output a summary table:

```
| File | Action | Coverage |
|------|--------|----------|
| README.md | Updated | Root project overview |
| src/EntityTagCaching/README.md | Created | Full API coverage |
| tests/README.md | Updated | Test execution guide |
```
