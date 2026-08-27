## Context

The domain declares `IMutationEngine` and `IMutationStrategy` and the parser produces operator nodes. This change implements the engine and strategies and the Application use case that coordinates them, following the PRD's Strategy Pattern (section 6.3).

## Goals / Non-Goals

**Goals:**
- An engine that traverses AST operator nodes and applies the right strategies.
- Logical and arithmetic strategies fully implemented; constant and complex-expression strategies stubbed (v2.0).
- Profile-driven strategy selection.

**Non-Goals:**
- Mutant compilation/execution validation (out of MVP scope).
- Constant and complex-expression mutation beyond stubs (v2.0).

## Decisions

- **Strategy Pattern** — each `IMutationStrategy` declares `CanApply(node)` and `Apply(node)`, letting the engine stay open for new operators. Matches the PRD directly.
- **Application-layer coordination** — `GenerateMutationsUseCase` orchestrates parse → strategy selection → mutation generation → validation, keeping the domain engine pure.
- **`ValidationService`** — rejects no-op mutations (mutated text equal to original) and confirms applicability before emission.
- **Profile as a gate** — the engine filters strategies against the profile's flag matrix rather than duplicating that matrix in the engine.

## Risks / Trade-offs

- [Naive operator mutation may produce syntactically invalid COBOL] → MVP scope accepts this (PRD section 2.2 excludes compile validation); structural validation is a documented future feature.
- [Constant/complex-expression strategies are stubs] → Guard them behind the profile matrix so they only surface in `high` (v2.0).

## Migration Plan

None — additive. Depends on `domain-model` and `type-cobol-parser`; feeds `export-plugin`.
