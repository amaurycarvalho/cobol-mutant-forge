## Why

The mutation engine, plugins, and CLI all operate on a shared conceptual model (COBOL programs, mutations, test cases, packages, mutation profiles). Without a centralized domain layer, each feature would duplicate these concepts and diverge. The PRD (sections 4.1 and 6) specifies these entities and interfaces explicitly, so they must be defined first as the foundation for all behavior.

## What Changes

- Define domain entities: `CobolProgram`, `Mutation`, `TestCase`, `MutantPackage`.
- Define value objects: `OperationType`, `MutationType`, `MutationProfile`.
- Define the `MutationProject` aggregate root.
- Define domain interfaces: `ICobolParser`, `IMutationStrategy`, `IMutationEngine`, `IImportPlugin`, `IExportPlugin`.
- Model the mutation profile matrix (low/medium/high) from the PRD.

## Capabilities

### New Capabilities
- `domain-model`: The core entities, value objects, aggregate root, and ports/interfaces that all other capabilities depend on.

## Impact

- Populates `src/CobolMutantForge.Domain/` (Entities, ValueObjects, Aggregates, Interfaces).
- Establishes the contract types that Application use cases and Infrastructure adapters implement.
- No behavior beyond type definitions and value-object invariants is introduced.
