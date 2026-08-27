# code-metrics-gate

## Purpose

Define automated, blocking enforcement of code quality metrics — cyclomatic complexity, maintainability index, and code duplication — integrated into the build and CI quality gate.

## Requirements

### Requirement: Cyclomatic complexity gate

The build SHALL enforce a maximum cyclomatic complexity of 10 per method by enabling the code-quality analyzer rule CA1502 at error severity.

#### Scenario: Method exceeds complexity threshold

- **WHEN** a method has a cyclomatic complexity greater than 10
- **THEN** the build reports a CA1502 error and fails

#### Scenario: All methods within threshold

- **WHEN** every method has a cyclomatic complexity of 10 or less
- **THEN** the build completes without CA1502 errors

### Requirement: Maintainability index gate

The build SHALL enforce a minimum maintainability index of 30 per member by enabling the code-quality analyzer rule CA1505 at error severity.

#### Scenario: Member below maintainability threshold

- **WHEN** a type, method, field, property, or event has a maintainability index below 30
- **THEN** the build reports a CA1505 error and fails

#### Scenario: All members within threshold

- **WHEN** every member has a maintainability index of 30 or greater
- **THEN** the build completes without CA1505 errors

### Requirement: Analyzer threshold configuration

The project SHALL carry a `CodeMetricsConfig.txt` file containing `CA1502: 10` and `CA1505: 30`, and SHALL mark it as `AdditionalFiles` so the analyzer rules fire at those thresholds.

#### Scenario: Threshold file is wired

- **WHEN** the project files are inspected
- **THEN** `CodeMetricsConfig.txt` exists, contains the CA1502 and CA1505 thresholds, and is referenced as an `AdditionalFiles` item

### Requirement: Analyzer rule enablement

The code-quality analyzer rules CA1502 and CA1505 SHALL be enabled via an `.editorconfig` file at error severity.

#### Scenario: Rules enabled at error severity

- **WHEN** the `.editorconfig` file is inspected
- **THEN** it declares `dotnet_diagnostic.CA1502.severity = error` and `dotnet_diagnostic.CA1505.severity = error`

### Requirement: Code duplication gate

The `Makefile` SHALL provide a `duplication` target that runs jscpd against the `src/` source tree and fails when duplicated code exceeds 10%.

#### Scenario: Duplication within threshold

- **WHEN** `make duplication` is run and duplicated code is 10% or less
- **THEN** the target completes successfully

#### Scenario: Duplication exceeds threshold

- **WHEN** `make duplication` is run and duplicated code exceeds 10%
- **THEN** the target exits with a non-zero status

### Requirement: Quality gate integration

The code duplication check SHALL run as part of the `make quality-gate` target.

#### Scenario: Duplication runs in the quality gate

- **WHEN** `make quality-gate` is run
- **THEN** it invokes the `duplication` target and fails if duplication exceeds the threshold
