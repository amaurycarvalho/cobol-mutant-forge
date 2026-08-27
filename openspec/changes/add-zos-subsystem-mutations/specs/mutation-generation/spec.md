## ADDED Requirements

### Requirement: CICS command mutation

The system SHALL mutate single-line CICS commands by replacing the command verb per a fixed mapping (`READ`→`READNEXT`/`READPREV`, `WRITE`→`REWRITE`, `XCTL`→`RETURN`/`LINK`) and by removing single-line `SYNCPOINT` statements.

#### Scenario: READ replaced with READNEXT

- **WHEN** a program contains `EXEC CICS READ FILE('CUSTFILE') INTO(WS-CUST) RIDFLD(WS-ID) END-EXEC`
- **THEN** a mutant is generated replacing `READ` with `READNEXT`

#### Scenario: XCTL replaced with RETURN

- **WHEN** a program contains `EXEC CICS XCTL PROGRAM('NEXT') END-EXEC`
- **THEN** a mutant is generated replacing `XCTL` with `RETURN`

### Requirement: IMS DL/I function mutation

The system SHALL mutate IMS DL/I calls by replacing the function code per a fixed mapping (`GU`→`GN`, `GN`→`GU`/`GNP`, `GNP`→`GN`, `REPL`→`DLET`/`ISRT`, `ISRT`→`REPL`, `DLET`→`REPL`).

#### Scenario: Get Unique replaced with Get Next

- **WHEN** a program contains `CALL "CBLTDLI" USING GU, CUSTPCB, WS-CUST-IO, WS-CUST-SSA`
- **THEN** a mutant is generated replacing `GU` with `GN`

### Requirement: ADABAS buffer mutation

The system SHALL mutate ADABAS direct calls by removing an optional buffer argument (format buffer or record buffer) from the `CALL 'ADABAS' USING ...` argument list.

#### Scenario: Optional buffer removed

- **WHEN** a program contains `CALL 'ADABAS' USING WS-CONTROL, WS-FORMAT, WS-RECORD`
- **THEN** a mutant is generated with the `WS-RECORD` argument removed

### Requirement: VSAM mutation

The system SHALL mutate VSAM constructs by replacing the `ACCESS MODE` value (`SEQUENTIAL`↔`RANDOM`↔`DYNAMIC`) in `SELECT ... ASSIGN` file control entries and by replacing file operation verbs (`WRITE`↔`REWRITE`).

#### Scenario: Access mode changed

- **WHEN** a program contains a `SELECT ... ASSIGN` entry with `ACCESS MODE IS SEQUENTIAL`
- **THEN** a mutant is generated replacing `SEQUENTIAL` with `RANDOM` or `DYNAMIC`

#### Scenario: WRITE replaced with REWRITE

- **WHEN** a program contains a `WRITE` file operation
- **THEN** a mutant is generated replacing `WRITE` with `REWRITE`

## MODIFIED Requirements

### Requirement: Mutation profile enforcement

The engine SHALL apply only the strategies enabled by the active mutation profile: `low` (logical + arithmetic only), `medium` (adds numeric constants), `high` (adds complex expressions, string constants, and the CICS/IMS/ADABAS/VSAM subsystem mutations).

#### Scenario: Low profile limits strategies

- **WHEN** mutations are generated under the `low` profile
- **THEN** only logical and arithmetic mutations are produced

#### Scenario: Medium profile includes constants

- **WHEN** mutations are generated under the `medium` profile
- **THEN** numeric constant mutations are produced in addition to logical and arithmetic mutations, and no subsystem mutations are produced

#### Scenario: High profile includes subsystem mutations

- **WHEN** mutations are generated under the `high` profile for a program containing subsystem statements
- **THEN** CICS, IMS DL/I, ADABAS, and VSAM mutations are produced in addition to operator, constant, and complex-expression mutations
