## ADDED Requirements

### Requirement: CobolProgram entity

The domain SHALL model a `CobolProgram` as an entity representing a single COBOL source program, identified by a program name, and carrying its source text, resolved copybooks, and an optional parsed AST representation.

#### Scenario: Program identity

- **WHEN** two `CobolProgram` instances share the same program name and source hash
- **THEN** they are considered the same program for identity purposes

### Requirement: Mutation entity

The domain SHALL model a `Mutation` entity that captures a single syntactical change: a unique id, the mutation type, the source line number, the original text, the mutated text, and the test cases that cover it.

#### Scenario: Mutation describes a concrete change

- **WHEN** a `Mutation` is instantiated for a logical operator change
- **THEN** it exposes the original text, the mutated text, and the line number where the change applies

### Requirement: Mutation type value object

The domain SHALL define a `MutationType` value object constrained to the supported operators: `AND`→`OR`, `OR`→`AND`, `NOT` insertion/removal, `+`↔`-`, `*`↔`/`, and constant replacement.

#### Scenario: Unknown mutation types are rejected

- **WHEN** a `MutationType` is constructed with an unsupported value
- **THEN** construction fails with a validation error

### Requirement: Mutation profile value object

The domain SHALL define a `MutationProfile` value object constrained to `low`, `medium`, and `high`, each carrying a boolean flag matrix for logical operators, arithmetic operators, complex expressions, numeric constants, and string constants per the PRD profile table.

#### Scenario: Profile matrix matches the PRD

- **WHEN** the `medium` profile is inspected
- **THEN** logical and arithmetic operators and numeric constants are enabled while complex expressions and string constants are disabled

### Requirement: MutantPackage entity

The domain SHALL model a `MutantPackage` entity bundling a collection of mutants, an optional manifest, and a report, with the ability to reference its source program.

#### Scenario: Package aggregates mutants

- **WHEN** mutants are added to a `MutantPackage`
- **THEN** the package exposes the aggregated collection and their count

### Requirement: MutationProject aggregate

The domain SHALL define a `MutationProject` aggregate root that ties a project name, its paths, its mutation profile, and the imported programs/test cases together as the unit of a generation run.

#### Scenario: Project is the root of a run

- **WHEN** a `MutationProject` is created from configuration and imported artifacts
- **THEN** it exposes its programs, test cases, and active mutation profile

### Requirement: Domain ports

The domain SHALL declare the ports that adapters and use cases implement: `ICobolParser`, `IMutationStrategy`, `IMutationEngine`, `IImportPlugin`, and `IExportPlugin`.

#### Scenario: Ports are infrastructure-agnostic

- **WHEN** the domain interfaces are inspected
- **THEN** none reference infrastructure or external-library types
