## MODIFIED Requirements

### Requirement: Mutation type value object

The domain SHALL define a `MutationType` value object constrained to the supported operators: `AND`→`OR`, `OR`→`AND`, `NOT` insertion/removal, `+`↔`-`, `*`↔`/`, constant replacement, and the z/OS subsystem mutations (CICS command, IMS DL/I function, ADABAS buffer, and VSAM access-mode/operation changes).

#### Scenario: Unknown mutation types are rejected

- **WHEN** a `MutationType` is constructed with an unsupported value
- **THEN** construction fails with a validation error

#### Scenario: Subsystem mutation types are supported

- **WHEN** a `MutationType` representing a CICS command change (e.g., `CicsReadToReadnext`) or an IMS DL/I function change (e.g., `ImsGuToGn`) is referenced
- **THEN** the value is a valid member of the `MutationType` set

### Requirement: Mutation profile value object

The domain SHALL define a `MutationProfile` value object constrained to `low`, `medium`, and `high`, each carrying a boolean flag matrix for logical operators, arithmetic operators, complex expressions, numeric constants, string constants, and the z/OS subsystem categories (CICS, IMS, ADABAS, VSAM) per the PRD profile table.

#### Scenario: Profile matrix matches the PRD

- **WHEN** the `medium` profile is inspected
- **THEN** logical and arithmetic operators and numeric constants are enabled while complex expressions, string constants, and subsystem mutations are disabled

#### Scenario: High profile enables subsystem mutations

- **WHEN** the `high` profile is inspected
- **THEN** the CICS, IMS, ADABAS, and VSAM flags are enabled in addition to the existing operator, constant, and complex-expression flags
