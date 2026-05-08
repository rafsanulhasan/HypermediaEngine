---
applyTo: "**"
description: "Testing and quality gate requirements for HypermediaEngine"
---

# Testing Requirements

- After implementing every feature or fix, run `dotnet test` to validate all tests pass.
- After all tests pass, run `dotnet stryker` to run mutation tests and verify code quality.
- Never skip either quality gate before finishing.
