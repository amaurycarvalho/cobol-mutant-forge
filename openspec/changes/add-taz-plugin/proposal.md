## Why

The IBM Test Accelerator for Z (TAZ) plugin is still an inert stub, so CobolMutantForge cannot consume the test artifacts (`zapp.json`, `.zdata`, results) that modern z/OS teams produce with the successor to ZUnit. Enabling TAZ as a test-case source closes the same gap for TAZ that `zunit-import` closes for ZUnit, and reuses the framework-neutral packaging structures already planned.

## What Changes

- **New capability** `taz-plugin`: a functional `TestAcceleratorPlugin` that imports `zapp.json` project config, `.zdata` (JSON) test data, and test results (JUnit/JSON), and exports mutant packages.
- **Config**: `MutationConfigDto.TestAccelerator` becomes a typed `TazPluginConfiguration` (empty defaults for backward compatibility) instead of a `Dictionary<string, object?>`.
- **Runtime ports stubbed**: `ITazCliExecutor`, `ITazRestApiClient`, and `ICodeCoverageCollector` are introduced as ports with "not yet supported" implementations (the `taz` CLI, ODE REST, and code coverage all require a z/OS runtime this offline CLI does not reach).
- **Deferred by design**: `.ztest` (proprietary, reverse-engineered), `zapp.yaml`, and all online/z/OS runtime integration remain unsupported stubs that record warnings.

## Capabilities

### New Capabilities

- `taz-plugin`: import and export of IBM Test Accelerator for Z artifacts (project config, JSON test data, test results) and the typed plugin configuration.

### Modified Capabilities

- `cli`: `plugin list` SHALL report `testaccelerator` as available rather than "unavailable (planned for v2.0)".
- `zunit-import`: the `TestAcceleratorPlugin` SHALL no longer be an inert stub; it performs real import/export, with runtime surfaces remaining inert.

## Impact

- **Infrastructure**: `TestAcceleratorPlugin`, new parsers (`TazProjectConfigParser`, `ZDataParser`, `TazResultParser`), new ports (`ITazCliExecutor`, `ITazRestApiClient`, `ICodeCoverageCollector`) with stub implementations.
- **Application/Config**: `TazPluginConfiguration`, `MutationConfigDto`, `DefaultConfigFactory`.
- **CLI**: `PluginCommand` availability status.
- **Dependencies**: build order depends on `expand-zunit-plugin` (reuses framework-neutral coverage map / manifest v2).
- **No new external dependencies** (JSON-only; no YAML library).
- **No breaking changes**: typed config defaults are empty and backward compatible.
