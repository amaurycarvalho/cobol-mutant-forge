## ADDED Requirements

### Requirement: Initial version

The project SHALL adopt `0.1.0` as its initial version across all version-bearing files.

#### Scenario: Makefile version

- **WHEN** the `Makefile` is inspected
- **THEN** it declares `VERSION ?= 0.1.0`

#### Scenario: Changelog baseline

- **WHEN** `CHANGELOG.md` is inspected
- **THEN** it references `0.1.0` as the initial release baseline (no `1.0.x` version remains)

### Requirement: English-only content

All tool-facing text, specifications, documentation, and workflow files SHALL be written in English.

#### Scenario: Documentation language

- **WHEN** the README, changelogs, skills, and workflows are inspected
- **THEN** no non-English prose remains

### Requirement: Single-CLI build tooling

The `Makefile` SHALL build, test, lint, and publish the single `CobolMutantForge.sln` solution without any service-image, Docker, or SonarQube-stack targets.

#### Scenario: No service or image targets

- **WHEN** the `Makefile` targets are listed
- **THEN** no `build-images`, per-service image, `sonar-up`, `sonar-down`, or `sonar-check` targets exist

#### Scenario: Build and test

- **WHEN** `make build` and `make test` are run
- **THEN** they operate on `CobolMutantForge.sln`

### Requirement: Continuous integration

The `ci.yml` workflow SHALL run a single quality gate (restore, lint, build, test) on pushes to `main` and pull requests, without a SonarCloud matrix or MCP integration-test job.

#### Scenario: CI pipeline

- **WHEN** the `ci.yml` workflow is inspected
- **THEN** it defines a quality-gate job for the single solution and no per-service or Docker-based jobs

### Requirement: Installable release artifacts

The `release.yml` workflow SHALL publish self-contained, single-file executables for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`, package each as an archive (`.zip` for Windows, `.tar.gz` otherwise), and attach them to a GitHub release so users can download and run the tool without installing the .NET SDK.

#### Scenario: Cross-platform publish

- **WHEN** a `v*` tag is pushed
- **THEN** the workflow produces an archive for each target runtime identifier

#### Scenario: Release assets attached

- **WHEN** the release is created
- **THEN** all platform archives are attached as release assets

#### Scenario: No .NET SDK required at runtime

- **WHEN** a user downloads a release archive and extracts it
- **THEN** the `cobol-mutant-forge` executable runs without a local .NET SDK (self-contained)

### Requirement: Release skill tooling

The OpenSpec release skills SHALL reference the CLI tool (not service images) and read the version from the `Makefile`.

#### Scenario: release-version skill

- **WHEN** the `release-version` skill is applied
- **THEN** it updates `VERSION` in the `Makefile` and no longer refers to service images

#### Scenario: release-push skill

- **WHEN** the `release-push` skill is applied
- **THEN** it creates and pushes the `v<version>` tag and release branch for the CLI tool

### Requirement: Template cleanup

Template-only artifacts that do not apply to a freeware CLI (the self-hosted SonarQube Docker Compose stack) SHALL be removed, and `.gitignore` SHALL drop obsolete image/stack entries.

#### Scenario: Sonar stack removed

- **WHEN** the repository is inspected
- **THEN** the `sonarqube/` directory no longer exists
