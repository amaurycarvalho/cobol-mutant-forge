## 1. Entities

- [ ] 1.1 Implement `CobolProgram` entity (name, source text, copybooks, AST, hash-based identity)
- [ ] 1.2 Implement `Mutation` entity (id, type, line, original, mutated, coverage)
- [ ] 1.3 Implement `TestCase` entity (id, inputs, expected outputs)
- [ ] 1.4 Implement `MutantPackage` entity (mutants collection, manifest, report)

## 2. Value objects

- [ ] 2.1 Implement `OperationType` (logical/arithmetic/constant/complex-expression)
- [ ] 2.2 Implement `MutationType` enum (AND→OR, OR→AND, NOT add/remove, +↔-, *↔/, constant)
- [ ] 2.3 Implement `MutationProfile` value object with the low/medium/high flag matrix

## 3. Aggregate root

- [ ] 3.1 Implement `MutationProject` aggregate (project name, paths, profile, programs, test cases)

## 4. Ports and interfaces

- [ ] 4.1 Define `ICobolParser`
- [ ] 4.2 Define `IMutationStrategy`
- [ ] 4.3 Define `IMutationEngine`
- [ ] 4.4 Define `IImportPlugin`
- [ ] 4.5 Define `IExportPlugin`

## 5. Verification

- [ ] 5.1 Confirm `dotnet build CobolMutantForge.Domain` succeeds with no external package references
- [ ] 5.2 Confirm value-object invariants throw on invalid construction
