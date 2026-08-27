## 1. Unit tests

- [ ] 1.1 Add Domain unit tests (value-object invariants, profile matrix, entity identity)
- [ ] 1.2 Add Application unit tests (mutation engine logical + arithmetic cases)
- [ ] 1.3 Add import/export unit tests where behavior is pure

## 2. BDD scenarios

- [ ] 2.1 Add `BDD/Features/MutationGeneration.feature` with logical and arithmetic scenarios
- [ ] 2.2 Add `BDD/Steps/` step definitions implementing the feature
- [ ] 2.3 Wire MTP v1 as the BDD runner

## 3. Stryker configuration

- [ ] 3.1 Add `stryker-config.json` with mutate paths, reporters, thresholds, and test project

## 4. Compliance

- [ ] 4.1 Ensure no xUnit1051 in-loop assertions (use `Assert.All`)

## 5. Verification

- [ ] 5.1 Run `dotnet test` and confirm the unit suite passes
- [ ] 5.2 Run the BDD scenarios and confirm they pass
- [ ] 5.3 Confirm coverage exceeds 80% for core logic
