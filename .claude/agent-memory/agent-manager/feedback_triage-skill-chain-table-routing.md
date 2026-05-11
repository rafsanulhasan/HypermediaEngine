---
name: triage-skill-chain-table-routing
description: The triage skill's Step 3 chain table is the source of truth for routing decisions; product-manager and agent-manager must appear as routable destinations for lifecycle, backlog, release, and memory operations.
type: feedback
---

The `triage` skill's Step 3 "Assign Agent Chains" table is the canonical routing index for the triage-agent. Every legitimate routing destination must be represented in this table — otherwise the triage-agent has no signal that the route exists.

Required rows beyond the basic feature/bug/security/tech-debt/test/design chains:

- Backlog / prioritization question → product-manager
- Release / milestone planning → product-manager → deploy skill
- Feature/TechDebt needing prioritization → product-manager → (then appropriate build chain)
- Agent / skill / command lifecycle → agent-manager
- Rules / instructions / hooks change → agent-manager
- Agent memory prune / audit / refresh → agent-manager

**Why:** Step 5 of the same skill flags PM consultation for Feature/TechDebt items, implying the PM is part of the flow, but earlier versions of the table omitted the PM entirely — the table is the routing source of truth and must match Step 5's expectations. Likewise, CLAUDE.md's File Ownership rule requires routing all agent/skill/command/rule/hook lifecycle through agent-manager, but earlier table versions had no entry for this, so the triage-agent could not route correctly.

**How to apply:** when reviewing or modifying the triage skill, ensure both `.claude/skills/triage/SKILL.md` and `.agents/skills/triage/SKILL.md` carry the full chain table including product-manager and agent-manager rows. Add new lifecycle/ownership routes to the table whenever a new owning agent is introduced.
