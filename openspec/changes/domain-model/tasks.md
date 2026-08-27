## 1. Entities

- [x] 1.1 Implement `CobolProgram` entity (name, source text, copybooks, AST, hash-based identity)
- [x] 1.2 Implement `Mutation` entity (id, type, line, original, mutated, coverage)
- [x] 1.3 Implement `TestCase` entity (id, inputs, expected outputs)
- [x] 1.4 Implement `MutantPackage` entity (mutants collection, manifest, report)

## 2. Value objects

- [x] 2.1 Implement `OperationType` (logical/arithmetic/constant/complex-expression)
- [x] 2.2 Implement `MutationType` enum (AND→OR, OR→AND, NOT add/remove, +↔-, *↔/, constant)
- [x] 2.3 Implement `MutationProfile` value object with the low/medium/high flag matrix

## 3. Aggregate root

- [x] 3.1 Implement `MutationProject` aggregate (project name, paths, profile, programs, test cases)

## 4. Ports and interfaces

- [x] 4.1 Define `ICobolParser`
- [x] 4.2 Define `IMutationStrategy`
- [x] 4.3 Define `IMutationEngine`
- [x] 4.4 Define `IImportPlugin`
- [x] 4.5 Define `IExportPlugin`

## 5. Verification

- [x] 5.1 Confirm `dotnet build CobolMutantForge.Domain` succeeds with no external package references
- [x] 5.2 Confirm value-object invariants throw on invalid construction
