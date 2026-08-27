## Why

The mutation engine today only mutates core COBOL operators (`AND`/`OR`/`NOT` and `+`/`-`/`*`/`/`). Real-world mainframe applications embed their business logic inside z/OS subsystem calls — **CICS**, **IMS DL/I**, **ADABAS**, and **VSAM** — which the current parser and mutation strategies are blind to. This means data-access, transaction-boundary, and program-control faults go undetected, undermining the tool's value for the exact programs it targets (per RFC-001).

## What Changes

- **Parser** (`TypeCobolParserAdapter`) emits new AST node kinds for subsystem statements: `CicsStatement`, `ImsDliStatement`, `AdabasStatement`, `VsamFileControlEntry`, and `VsamFileOperation`. Single-line constructs first; multi-line `EXEC CICS ... END-EXEC` is out of scope for this change.
- **Domain model** extends `MutationType` with subsystem mutation types (fine-grained, following the existing `XxxToYyy` convention) and adds subsystem flags to `MutationProfile` plus corresponding `OperationType` values.
- **Mutation strategies** are added for each subsystem, following the existing `IMutationStrategy` shape: `CicsCommandMutationStrategy`, `ImsDliMutationStrategy`, `AdabasMutationStrategy`, `VsamMutationStrategy`.
- **Engine wiring** registers the new strategies and gates them by the active profile's subsystem flags.
- **ADABAS scope is reduced**: only the `CALL 'ADABAS'` argument/buffer structure is mutated (e.g., removing an optional buffer). Command-code mutation (e.g., `L1`→`L2`) is deferred because the command code lives in a runtime control block, not in the static `CALL`.

## Capabilities

### New Capabilities

### Modified Capabilities

- `cobol-parsing`: parser SHALL expose subsystem statement nodes for CICS, IMS DL/I, ADABAS, and VSAM as discoverable AST nodes.
- `domain-model`: `MutationType` and `MutationProfile` SHALL be extended with subsystem mutation types and subsystem gating flags.
- `mutation-generation`: the engine SHALL generate subsystem mutations (CICS command, IMS DL/I function, ADABAS buffer, VSAM access-mode/operation) gated by the active profile.

## Impact

- **Domain**: `MutationType`, `OperationType`, `MutationProfile` (`ValueObjects/`), and their tests.
- **Infrastructure**: `TypeCobolParserAdapter` (`Parsers/`), new subsystem strategies under `Mutators/`, `MutationEngine` wiring.
- **Application/Config**: `MutationFlagsDto` and `DefaultConfigFactory` gain subsystem flags (default off), reflected in the generated JSON config.
- **Tests**: `TypeCobolParserAdapterTests`, `MutationStrategyTests`, `MutationEngineTests`, and BDD `MutationGeneration.feature`.
- **No breaking changes** to existing mutation behavior or serialized config (new fields default to false).
