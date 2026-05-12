---
name: agent-invocation skill rolled out as single source of truth for inter-agent invocation
description: Shared skill created in .claude/skills/ and .github/skills/; every agent (both platforms) carries a short Invocation Protocol pointer instead of duplicated invocation content.
type: project
---

The `agent-invocation` skill is the authoritative source for how any agent spawns or invokes another agent. Created on 2026-05-11 at both `.claude/skills/agent-invocation/SKILL.md` and `.github/skills/agent-invocation/SKILL.md`.

**Why:** Previously, invocation mechanics, routing rules, the SDLC chain, and the briefing checklist were duplicated across triage-agent, software-architect, and requirement-analyst — and the project's hook/linter was actively pruning these duplicate sections, leaving incomplete fragments. A shared skill removes the duplication and lets the linter stop fighting agent files.

**How to apply:**
- Any future change to invocation mechanics, routing rules, SDLC chain, or briefing checklist goes into the SKILL.md files only — never paste those sections into individual agent files.
- Every agent must carry a short (~3–5 line) `### Invocation Protocol` pointer in its definition that (1) names the skill, (2) cites the agent's role-specific handoff artifact in the chain, and (3) states the skill is authoritative.
- The pointer lives immediately before the `### Research Protocol` section in every agent file on both platforms.
- Per-role tailoring already applied: triage-agent (primary caller, uses with agent-selection), agent-manager (destination for lifecycle routing), research-assistant (destination for external knowledge), product-manager (loop with triage-agent), SDLC stages 1–6 each cite their forward handoff artifact.
