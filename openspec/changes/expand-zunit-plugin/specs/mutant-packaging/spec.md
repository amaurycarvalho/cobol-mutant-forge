## ADDED Requirements

### Requirement: Per-mutant test data generation

The exporter SHALL generate a `test-data.xml` for each mutated program, replicating the source test case's inputs and expected outputs so each mutant ships with its own test data for replay.

#### Scenario: Test data emitted per mutant

- **WHEN** a package with mutants and a source program with test cases is exported
- **THEN** each mutant is accompanied by a `test-data.xml` derived from the source test case data

### Requirement: Test data schema generation

The exporter SHALL generate a `test-data.xsd` schema describing the layout of the emitted mutant test data.

#### Scenario: Schema emitted

- **WHEN** mutant test data is generated
- **THEN** a `test-data.xsd` describing the test data layout is included in the package

### Requirement: Coverage map generation

The exporter SHALL generate a `coverage-map.json` mapping each mutation to the test entries that cover it.

#### Scenario: Coverage map emitted

- **WHEN** a package with mutations that carry covering test ids is exported
- **THEN** a `coverage-map.json` enumerates each mutation and its covering test entries

## MODIFIED Requirements

### Requirement: Manifest structure

The generated `manifest.json` SHALL include `mutantId`, `originalProgram`, `baseProgramHash`, `timestamp`, `mutationProfile`, a `mutations` array (id, type, line, original, mutated, testCaseCoverage), source/copybook resolution flags, and the optional `cicsContext`, `recordedEntries`, and `schemaVersion` fields.

#### Scenario: Manifest is well-formed

- **WHEN** a package is generated
- **THEN** the manifest contains all fields defined in the PRD manifest structure

#### Scenario: Manifest carries optional expansion fields

- **WHEN** a package is generated with CICS context and recorded entries available
- **THEN** the manifest includes `cicsContext`, `recordedEntries`, and `schemaVersion`, each empty when unavailable
