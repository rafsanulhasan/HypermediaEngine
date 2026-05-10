---
name: PR review tools belong on Claude code-reviewer
description: docker-mcp-gateway PR/issue management tools (non-merge) are intentionally Claude-only on code-reviewer; merge_pull_request remains Copilot-only
type: feedback
---

On 2026-05-11 the user added 11 docker-mcp-gateway PR and issue management tools to `.claude/agents/code-reviewer.md` (`add_comment_to_pending_review`, `add_issue_comment`, `add_reply_to_pull_request_comment`, `assign_copilot_to_issue`, `list_pull_requests`, `pull_request_read`, `pull_request_review_write`, `request_copilot_review`, `search_pull_requests`, `update_pull_request`, `update_pull_request_branch`) WITHOUT mirroring to `.github/agents/code-reviewer.agent.md`.

**Why:** Reaffirms the existing tool-divergence policy. The user explicitly excluded `merge_pull_request`, `vscode/*`, `github.vscode-pull-request-github/*`, `execute`, `edit`, and `todo` from the Claude-side addition — those remain Copilot/VS Code-only. The Claude code-reviewer is allowed to read PRs, comment, request reviews, and update PR metadata, but is NOT allowed to merge.

**How to apply:**
- When asked to add docker-mcp-gateway tools to a Claude agent, do not auto-propagate to the Copilot counterpart unless the user says so.
- Hard exclusion list for the Claude code-reviewer: `merge_pull_request`, `vscode/*`, `github.vscode-pull-request-github/*`, `execute`, `edit`, `todo`.
- Convert tool names from Copilot dotted form (`docker-mcp-gateway/foo`) to Claude MCP form (`mcp__docker-mcp-gateway__foo`) when porting.
