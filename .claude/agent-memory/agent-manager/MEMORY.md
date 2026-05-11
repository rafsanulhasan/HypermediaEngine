# Memory Index

- [Claude-only tool exclusion when syncing to Copilot](feedback_sync-tool-exclusions.md) — Strip Bash/Glob/Grep/Read/TodoWrite/WebFetch/WebSearch/PushNotification/ToolSearch when projecting tools: into .github/agents/*.agent.md.
- [Session memory protocol for agent-manager](feedback_session-memory-protocol.md) — Always load memory at session start and save new learnings at session end.
- [Triage skill chain table routing](feedback_triage-skill-chain-table-routing.md) — Step 3 of the triage skill must list product-manager and agent-manager rows for backlog, release, lifecycle, rules, hooks, and memory ops.
- [triage-agent uses agent-selection per-batch with four modes](project_triage-agent-selection-section.md) — Invoke once per triage cycle with full batch; respect the four orchestration modes; keep both platform files in sync.
- [research-assistant agent and research skill rolled out](project_research-assistant-rollout.md) — Read-only Opus research agent with context7-first source preference; uniform Research Protocol appended to all 10 existing agents on both platforms.
- [agent-invocation skill rolled out as single source of truth](project_agent-invocation-skill-rollout.md) — Shared skill at .claude/skills/ + .agents/skills/; every agent on both platforms carries a short Invocation Protocol pointer before Research Protocol instead of duplicated content.
- [Anti-Hallucination Protocol rolled out across all 11 agents](project_anti-hallucination-rollout.md) — Each agent on both platforms carries an "## Anti-Hallucination Protocol" section right after the role intro; research-assistant uses a tailored variant about not fabricating sources.
