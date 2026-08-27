## 1. Mutation strategies

- [x] 1.1 Implement `LogicalOperatorMutationStrategy` (AND↔OR, NOT insert/remove)
- [x] 1.2 Implement `ArithmeticOperatorMutationStrategy` (+↔-, *↔/)
- [x] 1.3 Implement `ConstantMutationStrategy` stub (v2.0)
- [x] 1.4 Implement `ComplexExpressionMutationStrategy` stub (v2.0)

## 2. Engine

- [x] 2.1 Implement `MutationEngine` realizing `IMutationEngine` (GenerateMutations, ApplyMutation, ValidateMutation)
- [x] 2.2 Filter strategies by the active mutation profile flag matrix
- [x] 2.3 Assign unique mutation ids and map test-case coverage

## 3. Application use case and validation

- [x] 3.1 Implement `GenerateMutationsUseCase`
- [x] 3.2 Implement `ValidationService` (reject no-op and inapplicable mutations)

## 4. Verification

- [x] 4.1 Generate mutations for `IF A > B AND C = D` under `medium` and assert the `OR` mutant is produced
- [x] 4.2 Generate mutations for `COMPUTE TOTAL = AMOUNT + TAX` and assert the `-` mutant is produced
- [x] 4.3 Assert `low` profile produces only logical and arithmetic mutations
