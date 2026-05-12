---
name: Skill creation routing pattern
description: How to break down and route "create a new skill" requests when a sibling skill exists as a template
type: feedback
---

When the user asks to create a new skill that mirrors an existing one (e.g., csharp-integration-testing modelled on csharp-unit-testing), the correct decomposition is:

1. Read the sibling skill in full as a template anchor.
2. Read any reference docs the user supplies (web fetch in parallel).
3. Identify the discriminating concerns vs the sibling — call them out explicitly in Phase 1 of the new skill (e.g., "no mocks; real infrastructure").
4. Reuse the sibling's structure (Phase 0 context, Phase 1 conventions, Phase 2 example, Phase 3 takeaways) — readers are already trained on it.
5. Write twin files: `.claude/skills/<name>/SKILL.md` and `.github/skills/<name>/SKILL.md` with identical content.
6. Update the consumer agent in twin files: `.claude/agents/<agent>.md` and `.github/agents/<agent>.agent.md`.

**Why:** The CLAUDE.md "Multi-Platform Agent Portability" rule mandates twin files. Forgetting the `.agents/` or `.github/agents/` copy ships a half-broken skill that only works on one platform.

**How to apply:** Whenever a request mentions creating or updating a skill or agent, plan four file writes from the start, not two. The work is artifact-only and falls under the testing-gate exception in `.claude/rules/testing.md` — no `dotnet test` or `dotnet stryker` needed.
