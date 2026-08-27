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

### [zunit-plugin](openspec/changes/zunit-plugin) Implement `ZUnitPlugin` implementing `IImportPlugin` (and ready to implement `IExportPlugin` for output).

#### Added

- Implement `ZUnitPlugin` implementing `IImportPlugin` (and ready to implement `IExportPlugin` for output).
- Parse `.xml` test data into `TestCase` objects and `.bzucfg` into `ZUnitConfig`.
- Load `.cbl` source into `CobolProgram` instances and resolve COPYBOOK dependencies.
- Produce a `ZUnitImportResult` with programs, test cases, config, copybooks, warnings, and validity.
- Add a `PluginBase` abstraction and the `TestAcceleratorPlugin` stub (v2.0 placeholder).

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

[Unreleased]: https://github.com/amaurycarvalho/cobol-mutant-forge/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/amaurycarvalho/cobol-mutant-forge/releases/tag/v0.2.0

See [CHANGELOG Archive](CHANGELOG-ARCHIVE.md) for older releases.
