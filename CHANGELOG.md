# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### [cli-commands](openspec/changes/cli-commands) Implement the CLI entry point (`Program.cs`) with System.CommandLine.

#### Added
- Implement the CLI entry point (`Program.cs`) with System.CommandLine.
- Implement commands: `InitCommand`, `GenerateCommand`, `ExportCommand`, `PluginCommand` (list).
- Wire `--help` and `--version` (reporting the tool version).
- Implement `ServiceCollectionExtensions` for dependency injection of parsers, plugins, engine, and use cases.
- Support `--quiet` across commands.

### [domain-model](openspec/changes/domain-model) Define domain entities: `CobolProgram`, `Mutation`, `TestCase`, `MutantPackage`.

#### Added
- Define domain entities: `CobolProgram`, `Mutation`, `TestCase`, `MutantPackage`.
- Define value objects: `OperationType`, `MutationType`, `MutationProfile`.
- Define the `MutationProject` aggregate root.
- Define domain interfaces: `ICobolParser`, `IMutationStrategy`, `IMutationEngine`, `IImportPlugin`, `IExportPlugin`.
- Model the mutation profile matrix (low/medium/high) from the PRD.

### [export-plugin](openspec/changes/export-plugin) Implement `MutantPackageExporter` realizing `IExportPlugin`.

#### Added
- Implement `MutantPackageExporter` realizing `IExportPlugin`.
- Generate `manifest.json` (PRD section 5.3.4) and `mutations-report.json` per package.
- Support both `zip` and `folder` output formats.
- Implement `ExportMutantsUseCase` in the Application layer.

### [mutation-engine](openspec/changes/mutation-engine) Implement `MutationEngine` realizing `IMutationEngine`.

#### Added
- Implement `MutationEngine` realizing `IMutationEngine`.
- Implement mutation strategies: `LogicalOperatorMutationStrategy` and `ArithmeticOperatorMutationStrategy` (plus `ConstantMutationStrategy` and `ComplexExpressionMutationStrategy` stubs for v2.0).
- Implement `GenerateMutationsUseCase` and `ValidationService` in the Application layer.
- Enforce the mutation profile matrix when deciding which strategies run.

### [testing-suite](openspec/changes/testing-suite) Add xUnit v3.2.2 unit tests for Domain and Application logic.

#### Added
- Add xUnit v3.2.2 unit tests for Domain and Application logic.
- Add MTP v1 BDD feature/scenario files (e.g., `MutationGeneration.feature`) with step definitions.
- Add `stryker-config.json` for mutation testing the tool.
- Enforce the xUnit1051 warning guidance (avoid assertions inside loops).

### [type-cobol-parser](openspec/changes/type-cobol-parser) Add the TypeCobol dependency to the Infrastructure project.

#### Added
- Add the TypeCobol dependency to the Infrastructure project.
- Implement `TypeCobolParserAdapter` conforming to `ICobolParser`.
- Map TypeCobol's parse tree onto the domain's minimal `AstNode` representation.
- Surface parser diagnostics (errors/warnings) to callers.
- Provide a fallback-friendly error path for unsupported constructs.

### [zunit-plugin](openspec/changes/zunit-plugin) Implement `ZUnitPlugin` implementing `IImportPlugin` (and ready to implement `IExportPlugin` for output).

#### Added
- Implement `ZUnitPlugin` implementing `IImportPlugin` (and ready to implement `IExportPlugin` for output).
- Parse `.xml` test data into `TestCase` objects and `.bzucfg` into `ZUnitConfig`.
- Load `.cbl` source into `CobolProgram` instances and resolve COPYBOOK dependencies.
- Produce a `ZUnitImportResult` with programs, test cases, config, copybooks, warnings, and validity.
- Add a `PluginBase` abstraction and the `TestAcceleratorPlugin` stub (v2.0 placeholder).

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

[Unreleased]: https://github.com/amaurycarvalho/cobol-mutant-forge/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/amaurycarvalho/cobol-mutant-forge/releases/tag/v0.1.0

See [CHANGELOG Archive](CHANGELOG-ARCHIVE.md) for older releases.
