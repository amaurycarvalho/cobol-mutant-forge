## Why

The repository's project metadata (README, Makefile, changelogs, OpenSpec skills, and GitHub Actions workflows) still describes the unrelated multi-service template (`agentic-fp-ai-mvp`: Docker images, per-service SonarCloud, MCP integration tests, `VERSION 1.0.1`). CobolMutantForge is a single freeware CLI tool, so this metadata is wrong, misleading, and blocks a correct release. All of it must be adapted to this project's reality.

## What Changes

- Rewrite `README.md` for the CobolMutantForge CLI (features, requirements, install, usage) in English.
- Rewrite `Makefile` for a single-solution CLI: `VERSION ?= 0.1.0`, `build`, `test`, `lint`, `clean`, `publish`, and a lean quality gate (no services, images, or SonarQube stack).
- Reset `CHANGELOG.md` to a clean `[Unreleased]` baseline and clear the template content from `CHANGELOG-ARCHIVE.md`, both in English.
- Update `.opencode/skills/changelog`, `.opencode/skills/release-version`, and `.opencode/skills/release-push` (including `release-push.sh`) to describe a single CLI tool rather than service images.
- Rewrite `.github/workflows/ci.yml` (single build/test/lint gate, no SonarCloud matrix, no MCP integration job).
- Rewrite `.github/workflows/release.yml` to publish self-contained, single-file executables for Windows, Linux, and macOS and attach them to the GitHub release so users can easily download and install the tool.
- Remove template-only artifacts (e.g., `sonarqube/docker-compose.yml`) and adjust `.gitignore` accordingly.

## Capabilities

### New Capabilities
- `project-metadata`: Build, test, release, and documentation tooling that reflects the single freeware CLI reality of CobolMutantForge.

## Impact

- `README.md`, `Makefile`, `CHANGELOG.md`, `CHANGELOG-ARCHIVE.md`.
- `.opencode/skills/changelog/SKILL.md`, `.opencode/skills/release-version/SKILL.md`, `.opencode/skills/release-push/SKILL.md`, `.opencode/skills/release-push/release-push.sh`.
- `.github/workflows/ci.yml`, `.github/workflows/release.yml`.
- Removes `sonarqube/` and updates `.gitignore`.
- All content written in English; initial version `0.1.0`.
