---
name: Multi-platform skill/agent file layout
description: HypermediaEngine maintains twin agent and skill files for Claude Code and Copilot/VS Code — both must be updated together
type: project
---

The HypermediaEngine repo supports Claude Code, GitHub Copilot CLI, and VS Code custom agents. Files come in pairs:

- Agents: `.claude/agents/<name>.md` (Claude) AND `.github/agents/<name>.agent.md` (Copilot/VS Code)
- Skills: `.claude/skills/<name>/SKILL.md` (Claude) AND `.agents/skills/<name>/SKILL.md` (Copilot/VS Code)

The Copilot agent files use a different frontmatter shape (lowercase tool aliases, `user-invocable` flag) and a much terser body (Responsibilities + Preferred Skills lists). The Claude agent files carry full behavioural prose, examples block, and skill-invocation blocks.

**Why:** This is a deliberate portability strategy — the repo is set up to be agent-platform-agnostic so the same role definitions drive all three IDEs.

**How to apply:** When routing skill or agent updates, always plan twin edits. When the user asks "add skill X to agent Y", that is two edits, not one. When the user asks "create skill X", that is two writes plus the twin agent updates if a consumer is registering it — typically four file operations total.
