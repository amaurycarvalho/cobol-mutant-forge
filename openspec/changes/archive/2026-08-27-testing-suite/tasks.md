## 1. Unit tests

- [x] 1.1 Add Domain unit tests (value-object invariants, profile matrix, entity identity)
- [x] 1.2 Add Application unit tests (mutation engine logical + arithmetic cases)
- [x] 1.3 Add import/export unit tests where behavior is pure

## 2. BDD scenarios

- [x] 2.1 Add `BDD/Features/MutationGeneration.feature` with logical and arithmetic scenarios
- [x] 2.2 Add `BDD/Steps/` step definitions implementing the feature
- [x] 2.3 Wire MTP v1 as the BDD runner

## 3. Stryker configuration

- [x] 3.1 Add `stryker-config.json` with mutate paths, reporters, thresholds, and test project

## 4. Compliance

- [x] 4.1 Ensure no xUnit1051 in-loop assertions (use `Assert.All`)

## 5. Verification

- [x] 5.1 Run `dotnet test` and confirm the unit suite passes
- [x] 5.2 Run the BDD scenarios and confirm they pass
- [x] 5.3 Confirm coverage exceeds 80% for core logic
