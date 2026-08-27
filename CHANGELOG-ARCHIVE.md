# Changelog Archive

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-08-27

### [add-metrics-gate](openspec/changes/archive/2026-08-27-add-metrics-gate) Add a `.editorconfig` that enables the built-in code-quality analyzers CA1502 (cyclomatic complexity) and CA1505 (maintainability index) as blocking build violations.

#### Added

- Add a `.editorconfig` that enables the built-in code-quality analyzers CA1502 (cyclomatic complexity) and CA1505 (maintainability index) as blocking build violations.
- Add a `CodeMetricsConfig.txt` (`CA1502: 10`, `CA1505: 30`) and wire it as `AdditionalFiles` so the rules fire at the intended thresholds.
- Add a `make duplication` target that runs jscpd with a 10% duplication threshold and fails the build when exceeded.

#### Changed

- Wire the duplication check into the existing quality gate and the `ci.yml` workflow.

### [domain-model](openspec/changes/archive/2026-08-27-domain-model) Define domain entities: `CobolProgram`, `Mutation`, `TestCase`, `MutantPackage`.

#### Added

- Define domain entities: `CobolProgram`, `Mutation`, `TestCase`, `MutantPackage`.
- Define value objects: `OperationType`, `MutationType`, `MutationProfile`.
- Define the `MutationProject` aggregate root.
- Define domain interfaces: `ICobolParser`, `IMutationStrategy`, `IMutationEngine`, `IImportPlugin`, `IExportPlugin`.
- Model the mutation profile matrix (low/medium/high) from the PRD.

### [type-cobol-parser](openspec/changes/archive/2026-08-27-type-cobol-parser) Add the TypeCobol dependency to the Infrastructure project.

#### Added

- Add the TypeCobol dependency to the Infrastructure project.
- Implement `TypeCobolParserAdapter` conforming to `ICobolParser`.
- Map TypeCobol's parse tree onto the domain's minimal `AstNode` representation.
- Surface parser diagnostics (errors/warnings) to callers.
- Provide a fallback-friendly error path for unsupported constructs.

## [0.1.0] - 2026-08-27

### [adapt-project-metadata](openspec/changes/archive/2026-08-27-adapt-project-metadata) Rewrite `README.md` for the CobolMutantForge CLI (features, requirements, install, usage) in English.

#### Changed
- Rewrite `README.md` for the CobolMutantForge CLI (features, requirements, install, usage) in English.
- Rewrite `Makefile` for a single-solution CLI: `VERSION ?= 0.1.0`, `build`, `test`, `lint`, `clean`, `publish`, and a lean quality gate (no services, images, or SonarQube stack).
- Reset `CHANGELOG.md` to a clean `[Unreleased]` baseline and clear the template content from `CHANGELOG-ARCHIVE.md`, both in English.
- Update `.opencode/skills/changelog`, `.opencode/skills/release-version`, and `.opencode/skills/release-push` (including `release-push.sh`) to describe a single CLI tool rather than service images.
- Rewrite `.github/workflows/ci.yml` (single build/test/lint gate, no SonarCloud matrix, no MCP integration job).
- Rewrite `.github/workflows/release.yml` to publish self-contained, single-file executables for Windows, Linux, and macOS and attach them to the GitHub release so users can easily download and install the tool.

#### Removed
- Remove template-only artifacts (e.g., `sonarqube/docker-compose.yml`) and adjust `.gitignore` accordingly.

### [bootstrap-project](openspec/changes/archive/2026-08-27-bootstrap-project) Create the `CobolMutantForge.sln` root solution targeting .NET 8.0 LTS.

#### Added
- Create the `CobolMutantForge.sln` root solution targeting .NET 8.0 LTS.
- Scaffold the five projects defined by the architecture: Domain, Application, Infrastructure, CLI, and Tests.
- Establish the Clean Architecture + DDD folder structure (`src/` + `tests/`).
- Introduce the base configuration file schema (`cobolmutantforge.json`) and its serialization contract.
- Wire foundational dependencies (System.CommandLine, System.Text.Json, xUnit v3.2.2, TypeCobol, logging) at the project level.
- Add solution-wide build configuration and a `.slnx`-agnostic directory layout under `src/` and `tests/`.

[0.2.0]: https://github.com/amaurycarvalho/cobol-mutant-forge/releases/tag/v0.2.0
[0.1.0]: https://github.com/amaurycarvalho/cobol-mutant-forge/releases/tag/v0.1.0

See main [CHANGELOG](CHANGELOG.md) for newer releases.
