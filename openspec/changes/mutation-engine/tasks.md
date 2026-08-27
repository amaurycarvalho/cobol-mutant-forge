## 1. Mutation strategies

- [ ] 1.1 Implement `LogicalOperatorMutationStrategy` (AND↔OR, NOT insert/remove)
- [ ] 1.2 Implement `ArithmeticOperatorMutationStrategy` (+↔-, *↔/)
- [ ] 1.3 Implement `ConstantMutationStrategy` stub (v2.0)
- [ ] 1.4 Implement `ComplexExpressionMutationStrategy` stub (v2.0)

## 2. Engine

- [ ] 2.1 Implement `MutationEngine` realizing `IMutationEngine` (GenerateMutations, ApplyMutation, ValidateMutation)
- [ ] 2.2 Filter strategies by the active mutation profile flag matrix
- [ ] 2.3 Assign unique mutation ids and map test-case coverage

## 3. Application use case and validation

- [ ] 3.1 Implement `GenerateMutationsUseCase`
- [ ] 3.2 Implement `ValidationService` (reject no-op and inapplicable mutations)

## 4. Verification

- [ ] 4.1 Generate mutations for `IF A > B AND C = D` under `medium` and assert the `OR` mutant is produced
- [ ] 4.2 Generate mutations for `COMPUTE TOTAL = AMOUNT + TAX` and assert the `-` mutant is produced
- [ ] 4.3 Assert `low` profile produces only logical and arithmetic mutations
