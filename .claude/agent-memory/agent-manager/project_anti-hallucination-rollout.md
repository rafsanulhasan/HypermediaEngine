---
name: Anti-Hallucination Protocol rolled out across all 11 agents
description: Each agent on both platforms carries an "Anti-Hallucination Protocol" section right after the role intro; research-assistant uses a tailored variant about not fabricating sources.
type: project
---

Added an `## Anti-Hallucination Protocol` section to every agent on both platforms (22 files total) on 2026-05-11.

**Why:** Defend against hallucinated API surfaces, file paths, version numbers, and project facts. Force agents to spawn `research-assistant` in parallel (one focused question per spawn) when uncertain about external/codebase facts, and to ask the user when uncertainty is about intent or acceptance criteria. Explicitly normalizes "I don't know — let me verify".

**How to apply:**
- Placement: inserted directly after each agent's role-intro paragraph and before the first `##` body heading (`## Behavioral Principles` on Claude side, `## Responsibilities` on Copilot side).
- Standard variant (10 agents): tells the agent to spawn `research-assistant` subagents in parallel via `Agent(...)` (Claude) or the `agent` tool (Copilot).
- Research-assistant variant: tailored — no self-spawn; instead "do not fabricate sources or citations, report Confidence: Low when authoritative info is unavailable, surface intent ambiguity as Open Question."
- When creating any new agent in the future, scaffold both platform files with this section in the same slot using the same variant rules.
- The two platform files (`.claude/agents/<name>.md` and `.github/agents/<name>.agent.md`) must stay in lockstep on this section — sync it like any other portable behavioral content (excluding only the Claude-only tool list per the existing sync exclusion rule).
