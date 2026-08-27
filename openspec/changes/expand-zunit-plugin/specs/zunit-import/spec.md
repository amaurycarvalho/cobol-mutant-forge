## ADDED Requirements

### Requirement: Test data schema import and validation

The system SHALL import `.xsd` test data schemas and, when a schema is present, validate the imported `.xml` test data against it, recording violations as warnings rather than aborting the import.

#### Scenario: Schema imported

- **WHEN** a ZUnit export directory contains a `.xsd` schema
- **THEN** the import result exposes the schema

#### Scenario: Test data validated against schema

- **WHEN** `.xml` test data is imported alongside a `.xsd` schema
- **THEN** schema violations are recorded as warnings and the import does not hard-fail

### Requirement: Recorded runtime data parsing

The system SHALL parse recorded runtime data XML (`batchrun`/`compileunit` structure) into recorded entries whose `IN` compile units map to test case inputs and `OUT` compile units map to expected outputs.

#### Scenario: Recorded entries extracted

- **WHEN** a debugger XML document with a `batchrun`/`compileunit` structure is parsed
- **THEN** recorded entries are materialized with their program name, I/O type, and data

#### Scenario: Recorded data maps to test cases

- **WHEN** recorded entries are imported
- **THEN** `IN` units contribute to a test case's inputs and `OUT` units to its expected outputs

### Requirement: Generation config parsing

The system SHALL parse AZUGEN generation configuration `.xml` into a generation config structure preserving generation options.

#### Scenario: Generation config extracted

- **WHEN** an AZUGEN `.xml` is imported
- **THEN** a generation config object is produced with the declared options

### Requirement: Runner results parsing

The system SHALL parse test runner results `.xml` into a runner results structure capturing pass/fail statistics.

#### Scenario: Runner results extracted

- **WHEN** a runner results `.xml` is imported
- **THEN** a runner results object is produced with the declared pass/fail information

### Requirement: CICS context import

The system SHALL read CICS context metadata from a dedicated `cics-context.json` file into a CICS context structure.

#### Scenario: CICS context read

- **WHEN** a `cics-context.json` file is present in the export directory
- **THEN** the import result exposes the region, pipeline, host, user ids, security, and SIT parameters

#### Scenario: Missing CICS context is non-fatal

- **WHEN** no `cics-context.json` file is present
- **THEN** the import succeeds with an empty CICS context and no error

### Requirement: Playback dataset stub

The system SHALL treat `.pb` playback datasets as unsupported, recording a warning on import instead of attempting to parse the binary format.

#### Scenario: Playback dataset reports unsupported

- **WHEN** a `.pb` file is encountered during import
- **THEN** a warning records that playback datasets are not yet supported and the import continues
