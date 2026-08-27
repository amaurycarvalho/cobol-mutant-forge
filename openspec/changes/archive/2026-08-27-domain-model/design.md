## Context

The bootstrap change established the project skeleton and configuration contract. This change fills `CobolMutantForge.Domain` with the conceptual model described in the PRD (sections 4.1, 5.3.2, 6.1–6.3). The domain is dependency-free: it must not reference external libraries or infrastructure so that use cases and adapters depend on it, not the reverse.

## Goals / Non-Goals

**Goals:**
- A complete set of entities, value objects, an aggregate root, and ports.
- Rich value objects with guarded invariants (mutation type, mutation profile).
- A mutation profile matrix that is the single source of truth for low/medium/high.

**Non-Goals:**
- Implementing mutation generation logic (owned by `mutation-engine`).
- Implementing parser/plugin adapters (owned by `type-cobol-parser` and `zunit-plugin`).
- Persistence or serialization concerns.

## Decisions

- **Value objects as C# records with guarded constructors** — immutability and validation at construction prevent invalid states from propagating. Alternatives: plain classes or enums. Enums are used for the small closed sets; records for multi-field VOs.
- **`MutationType` as an enum** — the operator set is fixed and small. `OperationType` mirrors the operation families (logical/arithmetic/constant/complex-expression).
- **`MutationProfile` as a value object (not a raw enum)** — a profile is really a flag matrix; modeling it as an object keeps the PRD table authoritative in one place.
- **Interfaces live in Domain** — following the ports-and-adapters style, so Application and Infrastructure both reference the domain contracts. This is the PRD's own structure (`Domain/Interfaces`).
- **AST is represented via an `AstNode` abstraction** — kept deliberately minimal here so `type-cobol-parser` can map TypeCobol's tree onto it.

## Risks / Trade-offs

- [Over-modeling the domain before the parser lands could force rework] → Keep `AstNode` minimal and let the parser change own any mapping detail.
- [Identity semantics for entities are easy to get wrong] → Use program name + source hash for `CobolProgram` equality, documented in the spec.

## Migration Plan

None — pure addition. The domain types are consumed by subsequent changes.
