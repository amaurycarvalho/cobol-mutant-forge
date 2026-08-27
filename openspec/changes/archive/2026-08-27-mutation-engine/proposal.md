## Why

Generating mutants is the core value of the tool. With the domain model, parser, and ZUnit import in place, the remaining piece is the engine that walks the AST, applies mutation strategies, and produces the `Mutation` list driven by the active profile. This is the heart of the PRD (section 6).

## What Changes

- Implement `MutationEngine` realizing `IMutationEngine`.
- Implement mutation strategies: `LogicalOperatorMutationStrategy` and `ArithmeticOperatorMutationStrategy` (plus `ConstantMutationStrategy` and `ComplexExpressionMutationStrategy` stubs for v2.0).
- Implement `GenerateMutationsUseCase` and `ValidationService` in the Application layer.
- Enforce the mutation profile matrix when deciding which strategies run.

## Capabilities

### New Capabilities
- `mutation-generation`: Applying logical and arithmetic mutations to COBOL programs according to a mutation profile.

## Impact

- Populates `src/CobolMutantForge.Application/UseCases/` and `Services/`.
- Depends on `domain-model` (strategies, engine interface) and `type-cobol-parser` (AST input).
- Produces the `Mutation` objects consumed by `export-plugin`.
