## ADDED Requirements

### Requirement: TAZ project config parsing

The system SHALL parse a TAZ project configuration file (`zapp.json`) into a typed project config structure via a tolerant parser.

#### Scenario: Project config extracted

- **WHEN** a `zapp.json` file is parsed
- **THEN** a project config object is produced with the declared settings

#### Scenario: Unsupported YAML config warns

- **WHEN** a `zapp.yaml` file is encountered
- **THEN** a warning records that YAML project config is not yet supported and the import continues

### Requirement: TAZ test data parsing

The system SHALL parse `.zdata` JSON test data files into test data structures usable as test cases.

#### Scenario: Test data extracted

- **WHEN** a `.zdata` JSON file is parsed
- **THEN** the test data is materialized with its declared values

### Requirement: TAZ result parsing

The system SHALL parse test results in JUnit XML and JSON formats into a results structure capturing pass/fail information.

#### Scenario: JUnit results parsed

- **WHEN** a JUnit XML results file is parsed
- **THEN** a results object is produced with the declared test outcomes

#### Scenario: JSON results parsed

- **WHEN** a results file produced with the `taz --json` flag is parsed
- **THEN** a results object is produced with the declared test outcomes

### Requirement: TAZ plugin import

The `TestAcceleratorPlugin` SHALL implement `IImportPlugin`, importing `zapp.json`, `.zdata` files, and test results, and recording warnings for unsupported artifacts (`.ztest`, `zapp.yaml`).

#### Scenario: TAZ import produces a result

- **WHEN** a directory of TAZ artifacts is imported
- **THEN** the result contains the project config, test data, and test results, with a validity flag

#### Scenario: Unsupported artifacts warn

- **WHEN** a `.ztest` file is encountered during import
- **THEN** a warning records that `.ztest` is not yet supported and the import does not hard-fail

### Requirement: TAZ plugin export

The `TestAcceleratorPlugin` SHALL implement `IExportPlugin`, packaging mutants using the framework-neutral manifest and coverage-map structures.

#### Scenario: TAZ export packages mutants

- **WHEN** the TAZ plugin is invoked to export a mutant package
- **THEN** a package is produced containing the mutant sources, manifest, and coverage map

### Requirement: Typed TAZ configuration

The system SHALL expose a typed `TazPluginConfiguration` with empty defaults, wired into the project configuration in place of the untyped `TestAccelerator` dictionary.

#### Scenario: Configuration typed with defaults

- **WHEN** the default configuration is generated
- **THEN** the TAZ configuration is a typed object with empty defaults, preserving backward compatibility

### Requirement: Runtime surfaces stubbed

The system SHALL declare `ITazCliExecutor`, `ITazRestApiClient`, and `ICodeCoverageCollector` ports whose implementations report "not yet supported".

#### Scenario: Runtime ports are inert

- **WHEN** any runtime port (CLI executor, REST client, coverage collector) is invoked
- **THEN** it reports that it is not yet supported rather than performing the operation
