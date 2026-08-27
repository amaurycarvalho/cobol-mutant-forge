## ADDED Requirements

### Requirement: Init command

The system SHALL provide an `init` command that creates a `cobolmutantforge.json` configuration file based on a directory structure, defaulting to the current directory and the `medium` profile.

#### Scenario: Init in current directory

- **WHEN** `cobol-mutant-forge init` is run with no directory argument
- **THEN** a `cobolmutantforge.json` is created in the current directory with default parameters and the `medium` profile

#### Scenario: Init with explicit profile

- **WHEN** `cobol-mutant-forge init --profile high` is run
- **THEN** the generated configuration uses the `high` profile

### Requirement: Generate command

The system SHALL provide a `generate` command that produces mutants from the project configuration, honoring `--config`, `--plugin`, `--output`, and `--quiet`.

#### Scenario: Generate from config

- **WHEN** `cobol-mutant-forge generate --config cobolmutantforge.json` is run
- **THEN** mutants are generated into the configured output directory

#### Scenario: Output override

- **WHEN** `cobol-mutant-forge generate --output ./mutants` is run
- **THEN** mutants are written to `./mutants`, overriding the configured output

### Requirement: Export command

The system SHALL provide an `export` command that packages generated mutants with `--source`, `--output`, and `--format` (`zip` | `folder`).

#### Scenario: Export to ZIP

- **WHEN** `cobol-mutant-forge export --source ./mutants --output ./packages --format zip` is run
- **THEN** a ZIP package is written to `./packages`

### Requirement: Plugin list command

The system SHALL provide a `plugin list` command listing all available plugins.

#### Scenario: Plugins listed

- **WHEN** `cobol-mutant-forge plugin list` is run
- **THEN** `zunit` and `testaccelerator` are listed with their availability status

### Requirement: Help and version

The system SHALL provide `--help` (detailed help for all commands) and `--version` (the tool version).

#### Scenario: Version reported

- **WHEN** `cobol-mutant-forge --version` is run
- **THEN** the current tool version is printed

#### Scenario: Help displayed

- **WHEN** `cobol-mutant-forge --help` is run
- **THEN** help text for all commands is displayed

### Requirement: Quiet mode

Every command SHALL accept `--quiet` to suppress informational messages while retaining errors.

#### Scenario: Quiet suppresses info

- **WHEN** any command is run with `--quiet`
- **THEN** only errors are printed, not informational messages
