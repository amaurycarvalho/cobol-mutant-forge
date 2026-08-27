## MODIFIED Requirements

### Requirement: Plugin list command

The system SHALL provide a `plugin list` command listing all available plugins with their availability status.

#### Scenario: Plugins listed

- **WHEN** `cobol-mutant-forge plugin list` is run
- **THEN** `zunit` and `testaccelerator` are listed with their availability status

#### Scenario: Test Accelerator reported available

- **WHEN** `cobol-mutant-forge plugin list` is run
- **THEN** `testaccelerator` is reported as `available` rather than `unavailable (planned for v2.0)`
