---
name: No Copilot tool bleed into Claude agents
description: User does not want Copilot-only tools (e.g. docker-mcp-gateway/merge_pull_request) carried into matching .claude/agents/*.md tools lists during sync
type: feedback
---

When syncing or porting agent definitions between `.github/agents/<name>.agent.md` (Copilot/VS Code) and `.claude/agents/<name>.md` (Claude Code), do not auto-mirror tools from Copilot into Claude. Tool sets are intentionally divergent per platform.

**Why:** On 2026-05-11 the user flagged that `mcp__docker-mcp-gateway__merge_pull_request` had been added to `.claude/agents/code-reviewer.md` because it existed (as `docker-mcp-gateway/merge_pull_request`) on the Copilot counterpart. The user explicitly does not want PR-merging capability on the Claude code-reviewer. SonarQube tools, by contrast, were added to Claude only and should NOT be propagated to Copilot. Tool divergence is deliberate.

**How to apply:**
- During `sync-agent`, derive structure (responsibilities, skills, body) from the Claude file but leave the Copilot tools list untouched. Never overwrite a target platform's `tools:` from the source platform.
- During `update-agent`, when a tools-list change is requested, confirm scope (Claude-only / Copilot-only / both) before propagating. Default to single-platform when the user names a single file path.
- Specifically: `merge_pull_request` belongs only on the Copilot code-reviewer; SonarQube `sonarqube_*` tools belong only on the Claude code-reviewer. Do not cross-propagate either direction without explicit user instruction.
