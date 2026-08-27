## 1. Build tooling

- [x] 1.1 Rewrite `Makefile`: `VERSION ?= 0.1.0`, `SOLUTION := CobolMutantForge.sln`
- [x] 1.2 Add `install`, `build`, `test`, `lint`, `clean`, and `publish` targets
- [x] 1.3 Add `publish-<rid>` targets producing self-contained single-file binaries
- [x] 1.4 Replace the quality gate with a lean `lint + test` gate and keep `mutation` as manual
- [x] 1.5 Remove service-image, `sonar-*`, and `test-integration` targets

## 2. Documentation

- [x] 2.1 Rewrite `README.md` for CobolMutantForge (features, requirements, install, usage, release install) in English
- [x] 2.2 Remove references to `agentic-fp-ai-mvp`, services, and SonarCloud

## 3. Changelogs

- [x] 3.1 Reset `CHANGELOG.md` to a clean English `[Unreleased]` baseline referencing `0.1.0`
- [x] 3.2 Clear the template content from `CHANGELOG-ARCHIVE.md` (English, empty of template entries)

## 4. OpenSpec skills

- [x] 4.1 Update `.opencode/skills/changelog/SKILL.md` wording for this project (English, no template specifics)
- [x] 4.2 Update `.opencode/skills/release-version/SKILL.md` to describe the CLI tool (drop service-image wording)
- [x] 4.3 Update `.opencode/skills/release-push/SKILL.md` for the CLI tool
- [x] 4.4 Update `.opencode/skills/release-push/release-push.sh` if it references template-specific paths

## 5. GitHub Actions

- [x] 5.1 Rewrite `.github/workflows/ci.yml` with a single quality-gate job
- [x] 5.2 Remove the SonarCloud matrix and MCP integration-test jobs
- [x] 5.3 Rewrite `.github/workflows/release.yml` to publish per-RID self-contained binaries and attach archives to the GitHub release

## 6. Template cleanup

- [x] 6.1 Delete the `sonarqube/` directory
- [x] 6.2 Update `.gitignore` to remove obsolete image/stack entries

## 7. Verification

- [x] 7.1 Run `make build test` successfully
- [x] 7.2 Confirm no non-English prose remains in README, changelogs, skills, and workflows
- [x] 7.3 Confirm `VERSION ?= 0.1.0` in the Makefile
