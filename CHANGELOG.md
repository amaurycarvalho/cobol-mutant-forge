# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0] - 2026-08-27

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

### [raise-mutation-score](openspec/changes/raise-mutation-score) Raise the Stryker.NET mutation score above 80% by expanding and strengthening the xUnit test suite.

#### Added

- Add new test coverage for the 607 "NoCoverage" mutants, prioritized by module: Domain entities/value objects/aggregates, Application use cases/services/configuration, then Infrastructure parsers/serialization/plugins/exporters/mutators.

#### Changed

- Expand and strengthen the xUnit test suite in `tests/CobolMutantForge.Tests` to kill at least 523 of the 683 surviving mutants, driving the mutation score above 80% (from 118 to 641+ killed).
- Strengthen existing assertions for the 76 "Survived" mutants (parsers, CLI commands, mutation profile) so existing tests detect the mutations they already execute.

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

[Unreleased]: https://github.com/amaurycarvalho/cobol-mutant-forge/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/amaurycarvalho/cobol-mutant-forge/releases/tag/v0.3.0

See [CHANGELOG Archive](CHANGELOG-ARCHIVE.md) for older releases.
