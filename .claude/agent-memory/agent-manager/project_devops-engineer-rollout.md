---
name: devops-engineer-rollout
description: devops-engineer agent and four CI/CD skills (nuget-package-deployment, github-ci-automation, github-cd-automation, sonarqube-pr-quality-gate) added; legacy deploy skill removed on 2026-05-13.
type: project
---

On 2026-05-13 the legacy `deploy` skill (only existed at `.github/skills/deploy/`) was deleted along with its `templates/release-notes.md`. Four replacement skills were created on both platforms: `nuget-package-deployment`, `github-ci-automation`, `github-cd-automation`, `sonarqube-pr-quality-gate`. A new agent `devops-engineer` was added on both platforms with memory dir scaffolded at `.claude/agent-memory/devops-engineer/`.

**Why:** the old `deploy` skill was a stub with no actionable content, lived only on one platform, and there was no agent that owned CI/CD or release execution. The product-manager's "Release Gate" used to terminate at `Skill("deploy")`, leaving an unowned handoff. The devops-engineer now picks up that handoff.

**How to apply:**
- All references to `Skill("deploy", ...)` or "deploy skill" in agent definitions, commands, prompts, triage routing tables, and product-planning have been rewritten to "hand off to devops-engineer". Generic "deploy" trigger words in triage Release-classification tables were left intact — they classify user intent, not skill names.
- product-manager (both platforms) Release Gate updated: step 5 now routes to `devops-engineer` and cites `nuget-package-deployment` and `github-cd-automation` skills.
- triage SKILL.md Step 3 chain table (both platforms) updated: `Release / milestone planning → product-manager → devops-engineer (NuGet publish + GitHub Release)`.
- The agent-manager memory entry [[triage-skill-chain-table-routing]] was updated to match the new routing.
- When future release-related skills are added (e.g., GitHub Container Registry, Azure deploys), they should be mapped to devops-engineer rather than spawning a new agent — single owner for all CI/CD and release infra.
