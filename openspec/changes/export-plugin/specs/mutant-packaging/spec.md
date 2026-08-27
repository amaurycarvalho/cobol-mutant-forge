## ADDED Requirements

### Requirement: Package generation

The system SHALL generate a package of mutants containing the mutated `.cbl` source files, a `manifest.json`, and a `mutations-report.json`.

#### Scenario: ZIP package generated

- **WHEN** the exporter is invoked with the `zip` format
- **THEN** a `.zip` archive is produced containing the mutated source, manifest, and report

#### Scenario: Folder output generated

- **WHEN** the exporter is invoked with the `folder` format
- **THEN** the same contents are written to the output directory without archiving

### Requirement: Manifest structure

The generated `manifest.json` SHALL include `mutantId`, `originalProgram`, `baseProgramHash`, `timestamp`, `mutationProfile`, a `mutations` array (id, type, line, original, mutated, testCaseCoverage), and source/copybook resolution flags.

#### Scenario: Manifest is well-formed

- **WHEN** a package is generated
- **THEN** the manifest contains all fields defined in the PRD manifest structure

### Requirement: Mutation report

The system SHALL generate a `mutations-report.json` listing every applied mutation with its details.

#### Scenario: Report enumerates mutations

- **WHEN** a package containing N mutations is generated
- **THEN** the report lists exactly N mutation entries

### Requirement: Source and copybook inclusion

The exporter SHALL record whether the original source was copied and whether copybooks were resolved in the manifest.

#### Scenario: Resolution flags recorded

- **WHEN** a package is generated
- **THEN** the manifest records `sourceCopied` and `copybooksResolved` flags accurately
