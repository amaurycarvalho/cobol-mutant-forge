## Why

Generated mutants are only useful if they can be handed back to a developer for manual import into CICS Explorer. The PRD specifies package generation (ZIP) with a `manifest.json` and a `mutations-report.json`. This change implements the exporter side of the plugin architecture.

## What Changes

- Implement `MutantPackageExporter` realizing `IExportPlugin`.
- Generate `manifest.json` (PRD section 5.3.4) and `mutations-report.json` per package.
- Support both `zip` and `folder` output formats.
- Implement `ExportMutantsUseCase` in the Application layer.

## Capabilities

### New Capabilities
- `mutant-packaging`: Packaging generated mutants (`.cbl` files + manifest + report) into a ZIP (or folder) for manual CICS import.

## Impact

- Populates `src/CobolMutantForge.Infrastructure/Exporters/` and `src/CobolMutantForge.Application/UseCases/`.
- Depends on `domain-model` (MutantPackage) and `mutation-engine` (Mutation output).
- Consumed by `cli-commands` (`export` command).
