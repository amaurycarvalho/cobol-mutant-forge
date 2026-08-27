# cobol-parsing

## Purpose

Define the COBOL source parsing capability: parsing source text into a structured AST via an adapter implementing `ICobolParser`, exposing operator nodes for mutation, and keeping comments and string literals out of the mutation candidate set.

## Requirements

### Requirement: COBOL source parsing

The system SHALL parse COBOL source text into a structured AST using TypeCobol via an adapter implementing `ICobolParser`.

#### Scenario: Valid COBOL parses successfully

- **WHEN** a valid COBOL program is passed to the parser adapter
- **THEN** a populated AST is returned with no fatal errors

#### Scenario: Invalid COBOL reports diagnostics

- **WHEN** COBOL containing a syntax error is parsed
- **THEN** the adapter returns diagnostics describing the error and the offending location

### Requirement: Operator node exposure

The parsed AST SHALL expose nodes for logical operators (`AND`, `OR`, `NOT`) and arithmetic operators (`+`, `-`, `*`, `/`) so that mutation strategies can traverse and mutate them.

#### Scenario: Logical operators are discoverable

- **WHEN** a program containing `IF A > 0 AND CUSTOMER-ACTIVE` is parsed
- **THEN** the AST contains a discoverable node for the `AND` operator

#### Scenario: Arithmetic operators are discoverable

- **WHEN** a program containing `COMPUTE TOTAL = AMOUNT + TAX` is parsed
- **THEN** the AST contains a discoverable node for the `+` operator

### Requirement: Comment and literal safety

The parser adapter SHALL NOT expose nodes corresponding to operators inside comments or string literals as mutation candidates.

#### Scenario: Operators in comments are ignored

- **WHEN** a program contains `* AND OR +` inside a comment line
- **THEN** those tokens are not surfaced as mutation candidate nodes
