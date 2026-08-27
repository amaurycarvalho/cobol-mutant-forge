## Why

The Stryker.NET mutation score is 14.73% (118 killed out of 801 detectable mutants), far below the 80% target. 683 mutants survive: 76 are covered by tests but their assertions are too weak ("Survived"), and 607 are not exercised by any test ("NoCoverage"). This change raises the mutation score above 80% by killing the surviving mutants across the full solution scope (Route A).

## What Changes

- Expand and strengthen the xUnit test suite in `tests/CobolMutantForge.Tests` to kill at least 523 of the 683 surviving mutants, driving the mutation score above 80% (from 118 to 641+ killed).
- Strengthen existing assertions for the 76 "Survived" mutants (parsers, CLI commands, mutation profile) so existing tests detect the mutations they already execute.
- Add new test coverage for the 607 "NoCoverage" mutants, prioritized by module: Domain entities/value objects/aggregates, Application use cases/services/configuration, then Infrastructure parsers/serialization/plugins/exporters/mutators.
- No production code changes; this change is test-only.

## Capabilities

### New Capabilities

<!-- none -->

### Modified Capabilities

- `testing-qa`: adds a mutation-score gate requirement — the Stryker.NET mutation score SHALL exceed 80% across the full solution, and the test suite SHALL kill the surviving mutants identified in the mutation report.

## Impact

- Affected code: `tests/CobolMutantForge.Tests/**` (xUnit test files only). No source or `*.csproj` changes.
- Existing test files expanded: `MutationEngineTests`, `MutationStrategyTests`, `TypeCobolParserAdapterTests`, `ZUnitConfigParserTests`, `ZUnitXmlParserTests`, `ZUnitPluginTests`, `MutantPackageExporterTests`, `PackageManifestReaderTests`, `DomainModelTests`, `CliIntegrationTests`, `TestAcceleratorPluginTests`.
- New test files added per uncovered module (Configuration DTOs, Services, Aggregates, Ast, Interfaces, CLI Commands).
- Verification command: `make mutation` (Stryker.NET) expected to report a score above 80%.
