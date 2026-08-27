## ADDED Requirements

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
