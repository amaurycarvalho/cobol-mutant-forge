## ADDED Requirements

### Requirement: Logical operator mutation

The system SHALL mutate logical operators by replacing `AND` with `OR`, replacing `OR` with `AND`, removing `NOT`, and inserting `NOT`.

#### Scenario: AND replaced with OR

- **WHEN** a program contains `IF AMOUNT > 0 AND CUSTOMER-ACTIVE`
- **THEN** a mutant is generated whose condition reads `IF AMOUNT > 0 OR CUSTOMER-ACTIVE`

#### Scenario: NOT removal and insertion

- **WHEN** a program contains a `NOT (condition)` expression
- **THEN** both a `NOT`-removed mutant and, where applicable, a `NOT`-inserted mutant are generated

### Requirement: Arithmetic operator mutation

The system SHALL mutate arithmetic operators by replacing `+` with `-`, `-` with `+`, `*` with `/`, and `/` with `*`.

#### Scenario: Addition replaced with subtraction

- **WHEN** a program contains `COMPUTE TOTAL = AMOUNT + TAX`
- **THEN** a mutant is generated replacing `+` with `-`

#### Scenario: Multiplication replaced with division

- **WHEN** a program contains an expression using `*`
- **THEN** a mutant is generated replacing `*` with `/`

### Requirement: Mutation profile enforcement

The engine SHALL apply only the strategies enabled by the active mutation profile: `low` (logical + arithmetic only), `medium` (adds numeric constants), `high` (adds complex expressions and string constants).

#### Scenario: Low profile limits strategies

- **WHEN** mutations are generated under the `low` profile
- **THEN** only logical and arithmetic mutations are produced

#### Scenario: Medium profile includes constants

- **WHEN** mutations are generated under the `medium` profile
- **THEN** numeric constant mutations are produced in addition to logical and arithmetic mutations

### Requirement: Mutation uniqueness and coverage

Each generated `Mutation` SHALL be uniquely identifiable and SHALL record the test cases whose inputs cover the mutated expression.

#### Scenario: Mutations carry coverage

- **WHEN** mutations are generated for a program with associated test cases
- **THEN** each mutation references the test cases covering its expression

### Requirement: Mutation validation

The system SHALL validate that a mutation is applicable before emitting it, returning false for mutations that would not change the program.

#### Scenario: No-op mutation rejected

- **WHEN** a mutation strategy would produce output identical to the original text
- **THEN** the mutation is rejected as invalid
