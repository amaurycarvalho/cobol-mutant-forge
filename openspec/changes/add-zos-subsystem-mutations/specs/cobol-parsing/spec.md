## ADDED Requirements

### Requirement: Subsystem statement exposure

The parsed AST SHALL expose discoverable nodes for z/OS subsystem constructs: CICS commands (`EXEC CICS ... END-EXEC` on a single line), IMS DL/I calls (`CALL "CBLTDLI"`/`"CEETDLI" USING ...`), ADABAS direct calls (`CALL 'ADABAS' USING ...`), and VSAM file control entries and file operations (`SELECT ... ASSIGN ...`, `READ`/`WRITE`/`REWRITE`/`DELETE`/`START`).

#### Scenario: CICS statements are discoverable

- **WHEN** a program containing `EXEC CICS READ FILE('CUSTFILE') INTO(WS-CUST-REC) RIDFLD(WS-CUST-ID) END-EXEC` is parsed
- **THEN** the AST contains a `CicsStatement` node whose command name is `READ` with option nodes for `FILE`, `INTO`, and `RIDFLD`

#### Scenario: IMS DL/I calls are discoverable

- **WHEN** a program containing `CALL "CBLTDLI" USING GU, CUSTPCB, WS-CUST-IO, WS-CUST-SSA` is parsed
- **THEN** the AST contains an `ImsDliStatement` node whose function code is `GU` and whose using arguments are exposed

#### Scenario: ADABAS calls are discoverable

- **WHEN** a program containing `CALL 'ADABAS' USING WS-CONTROL, WS-FORMAT, WS-RECORD` is parsed
- **THEN** the AST contains an `AdabasStatement` node whose buffer arguments are exposed

#### Scenario: VSAM file operations are discoverable

- **WHEN** a program containing `READ CUST-FILE RECORD INTO WS-CUST-REC KEY IS WS-CUST-ID` is parsed
- **THEN** the AST contains a `VsamFileOperation` node whose operation is `READ` and whose key expression is exposed
