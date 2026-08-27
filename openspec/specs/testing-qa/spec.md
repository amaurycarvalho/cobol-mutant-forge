# testing-qa

## Purpose

Define the automated quality-assurance strategy: xUnit v3.2.2 unit tests, MTP v1 BDD scenarios, Stryker.NET mutation testing configuration, and xUnit1051 assertion guidance.

## Requirements

### Requirement: Unit test coverage

The project SHALL include xUnit v3.2.2 unit tests covering Domain and Application logic, targeting greater than 80% coverage of the core mutation and import behavior.

#### Scenario: Engine mutation test

- **WHEN** the unit test suite runs against `MutationEngine`
- **THEN** the logical-operator mutation scenario passes and asserts the expected mutant count and content

#### Scenario: Profile matrix test

- **WHEN** the mutation profile matrix is tested
- **THEN** each profile's enabled/disabled flags match the PRD table

### Requirement: BDD scenarios

The project SHALL include MTP v1 BDD feature files and step definitions covering mutation generation for logical and arithmetic operators.

#### Scenario: Logical operator mutant

- **WHEN** the `MutationGeneration` feature scenario for a logical operator runs
- **THEN** a mutant replacing `AND` with `OR` is generated and saved as a `.cbl` file

#### Scenario: Arithmetic operator mutant

- **WHEN** the `MutationGeneration` feature scenario for an arithmetic operator runs
- **THEN** a mutant replacing `+` with `-` is generated

### Requirement: Stryker.NET configuration

The project SHALL provide a `stryker-config.json` configuring Stryker.NET to mutate the Domain and Application projects (excluding tests) with HTML and progress reporters and high/low thresholds of 80/60.

#### Scenario: Stryker configuration present

- **WHEN** the repository is inspected
- **THEN** `stryker-config.json` exists with the mutate paths, reporters, and thresholds from the PRD

### Requirement: Mutation score gate

The project SHALL maintain a Stryker.NET mutation score greater than 80% across the full solution. The score SHALL be measured over detectable mutants (killed + survived + no-coverage), and the test suite SHALL kill the surviving mutants identified in the mutation report so the score exceeds the 80% threshold.

#### Scenario: Full-solution score above 80%

- **WHEN** `make mutation` runs Stryker.NET against the full solution
- **THEN** the reported final mutation score is greater than 80%

#### Scenario: Survived mutants are killed

- **WHEN** a mutant is covered by a test but survives because its assertion is weak
- **THEN** the test asserts the concrete observable output so the mutant is reported as killed

#### Scenario: Uncovered mutants are covered

- **WHEN** a module has mutants reported as "NoCoverage"
- **THEN** tests are added that exercise those lines, reducing the "NoCoverage" count and killing those mutants

#### Scenario: Mutants excluded from the score are not targeted

- **WHEN** a mutant is reported as "CompileError" or "Ignored"
- **THEN** it is not counted toward the mutation score and requires no new test

### Requirement: xUnit1051 compliance

Tests SHALL avoid assertion calls inside loops, using `Assert.All` where a collection must be asserted per item.

#### Scenario: No in-loop assertions

- **WHEN** the test suite is analyzed for the xUnit1051 warning
- **THEN** no assertion occurs directly inside a loop
