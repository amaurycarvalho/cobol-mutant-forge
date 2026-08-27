## ADDED Requirements

### Requirement: ZUnit object consumption

The system SHALL import ZUnit-exported objects: `.xml` test data, `.bzucfg` configuration, `.cbl` COBOL source, and COPYBOOK files, through a plugin implementing `IImportPlugin`.

#### Scenario: Import produces a valid result

- **WHEN** a directory of ZUnit exports is imported
- **THEN** the result contains the programs, test cases, configuration, copybooks, and a validity flag

#### Scenario: Import records warnings

- **WHEN** an artifact is malformed or unsupported during import
- **THEN** the result's warnings list records the issue and the import does not hard-fail

### Requirement: XML test data parsing

The system SHALL parse ZUnit `.xml` test data files into `TestCase` objects capturing inputs and expected outputs.

#### Scenario: Test case extraction

- **WHEN** a ZUnit `.xml` export is parsed
- **THEN** each test case record is materialized as a `TestCase` with its inputs and expected outputs

### Requirement: ZUnit configuration parsing

The system SHALL parse `.bzucfg` configuration files into a `ZUnitConfig` object identifying test parameters and context.

#### Scenario: Configuration extraction

- **WHEN** a `.bzucfg` file is parsed
- **THEN** a `ZUnitConfig` is produced with the declared test parameters

### Requirement: Copybook resolution

The system SHALL resolve COPYBOOK references for imported `.cbl` programs from the configured copybook directory.

#### Scenario: Copybooks resolved

- **WHEN** a program references COPYBOOKs present in the copybook directory
- **THEN** the program's resolved copybooks are populated

#### Scenario: Missing copybook warns

- **WHEN** a referenced COPYBOOK cannot be found
- **THEN** a warning is recorded and the program remains importable

### Requirement: Test Accelerator stub

The system SHALL provide a `TestAcceleratorPlugin` stub implementing `PluginBase`, `IImportPlugin`, and `IExportPlugin`, marked as under development for v2.0.

#### Scenario: Stub is inert

- **WHEN** the Test Accelerator stub is invoked
- **THEN** it reports that it is not yet supported rather than performing an import/export
