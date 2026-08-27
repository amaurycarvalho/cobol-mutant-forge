## Why

The PRD requires a comprehensive QA strategy: xUnit v3.2.2 unit tests (>80% coverage), MTP v1 BDD scenarios, and Stryker.NET mutation testing of the tool itself. This change establishes the test suite and the Stryker configuration that prove the previous capabilities behave as specified.

## What Changes

- Add xUnit v3.2.2 unit tests for Domain and Application logic.
- Add MTP v1 BDD feature/scenario files (e.g., `MutationGeneration.feature`) with step definitions.
- Add `stryker-config.json` for mutation testing the tool.
- Enforce the xUnit1051 warning guidance (avoid assertions inside loops).

## Capabilities

### New Capabilities
- `testing-qa`: Automated unit tests (xUnit), BDD scenarios (MTP), and Stryker.NET mutation testing configuration.

## Impact

- Populates `tests/CobolMutantForge.Tests/` (Unit, BDD, stryker-config.json).
- Depends on `domain-model`, `mutation-engine`, and `zunit-plugin` for testable behavior.
- CI integration of tests is finalized in `adapt-project-metadata`.
