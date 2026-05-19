---
name: project-marten-test-breakage
description: MartenQueryableRules test files have compilation errors (Weasel.Core.AutoCreate missing, SortDirection ambiguity) that block whole test project. Pre-existing issue from parallel SQA agent.
metadata:
  type: project
---

`tests/HypermediaEngine.Tests/Rules/MartenQueryableRules/` contains broken files added by a parallel SQA agent:
- `MartenFixture.cs` — `Weasel.Core.AutoCreate` not found (missing assembly or wrong namespace in current Marten version)
- `MartenQueryableSortingRuleTests.cs` — `SortDirection` ambiguous between `Shouldly.SortDirection` and `HypermediaEngine.Requests.Sorting.SortDirection`

**Why:** These were added in an untracked state (parallel work on same branch). They prevent `dotnet build` of the test project.

**How to apply:** When running test sessions, check if MartenQueryableRules files compile. If not, the whole test project cannot build and `dotnet test` / `dotnet stryker` will fail. Report to user before running gates.
