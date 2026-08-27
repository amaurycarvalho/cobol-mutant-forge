# RFC: AST Expansion for COBOL Mutation Engine – z/OS Ecosystem Integration

**Status:** Draft  
**Author:** Mutation Engine Team  
**Date:** 2026-08-27  
**Category:** Language Engineering / AST Design  

---

## 1. Executive Summary

This RFC proposes an expansion of the COBOL Abstract Syntax Tree (AST) to support mutation testing across the full IBM z/OS enterprise ecosystem. The current AST implementation covers core COBOL language constructs but lacks semantic awareness of z/OS-specific subsystems: **CICS (Customer Information Control System)**, **IMS (Information Management System)**, **ADABAS**, and **VSAM (Virtual Storage Access Method)**. This expansion enables the mutation engine to generate meaningful mutants in real-world mainframe COBOL applications where business logic is deeply intertwined with these subsystems.

---

## 2. Background and Motivation

Enterprise COBOL applications on z/OS rarely exist in isolation. They interact with:

- **CICS** – Transaction processing monitor handling online applications
- **IMS** – Hierarchical database and transaction management system
- **ADABAS** – High-performance database system with direct call interface
- **VSAM** – Virtual Storage Access Method for file-based data storage

A mutation engine that only mutates `IF` conditions and arithmetic expressions fails to detect faults in data access logic, transaction boundaries, and subsystem interaction patterns. This RFC addresses that gap.

---

## 3. COBOL Language Foundation (IBM Enterprise COBOL for z/OS)

### 3.1 Core Operator Support (Already Supported)

The existing mutation engine supports the following COBOL operators as documented in the *IBM Enterprise COBOL for z/OS Language Reference*:

| Category | Operators | Notes |
|:---|:---|:---|
| Arithmetic (binary) | `+`, `-`, `*`, `/`, `**` | 5 binary operators |
| Arithmetic (unary) | `+`, `-` | Algebraic sign operators |
| Logical | `AND`, `OR`, `NOT` | Boolean operators |

**Precedence rules** (evaluation order):
1. Unary `+` or `-` (algebraic sign)
2. `**` (exponentiation)
3. `/` or `*` (division or multiplication)
4. Binary `+` or `-` (addition or subtraction)

### 3.2 AST Node Hierarchy (Expanded)

```
AstNode (abstract)
├── ExpressionNode (abstract)
│   ├── BinaryExpressionNode
│   │   ├── ArithmeticExpressionNode
│   │   │   ├── AddExpression
│   │   │   ├── SubtractExpression
│   │   │   ├── MultiplyExpression
│   │   │   ├── DivideExpression
│   │   │   └── ExponentiateExpression  [NEW]
│   │   └── LogicalExpressionNode
│   │       ├── AndExpression
│   │       ├── OrExpression
│   │       └── NotExpression          [unary]
│   ├── ConstantExpressionNode
│   │   ├── NumericConstant
│   │   ├── AlphanumericConstant
│   │   └── FigurativeConstant         (ZERO, SPACES, HIGH-VALUES, etc.)
│   └── VariableReferenceNode
│       ├── DataNameReference
│       └── SpecialRegisterReference   (ADDRESS OF, RETURN-CODE, etc.)
├── StatementNode (abstract)
│   ├── ConditionalStatementNode
│   │   ├── IfStatement
│   │   ├── EvaluateStatement
│   │   └── PerformStatement
│   ├── SubsystemStatementNode         [NEW - see Section 4]
│   │   ├── CicsStatement              [NEW]
│   │   ├── ImsStatement               [NEW]
│   │   ├── AdabasStatement            [NEW]
│   │   └── VsamStatement              [NEW]
│   └── DataAccessStatementNode        [NEW]
│       ├── ReadStatement
│       ├── WriteStatement
│       ├── RewriteStatement
│       └── DeleteStatement
└── ProgramNode
    ├── IdentificationDivision
    ├── EnvironmentDivision
    ├── DataDivision
    └── ProcedureDivision
```

---

## 4. Subsystem-Specific AST Expansion

### 4.1 CICS (Customer Information Control System)

#### 4.1.1 Documentation Reference

CICS Transaction Server for z/OS provides the command-level API documented in:
- *CICS Application Programming Reference* (SC34-7159)
- *CICS Application Programming Guide* (SC34-7158)
- *API (EXEC CICS) Reference*

#### 4.1.2 Syntax Pattern

CICS commands in COBOL follow the EXEC CICS format:

```cobol
EXEC CICS command-name command-options END-EXEC
```

- Command name is a verb (e.g., `READ`, `WRITE`, `START`, `XCTL`)
- Options are keyword-value pairs (e.g., `FILE('filename')`, `INTO(ws-area)`)
- Space is the word separator; commas and semicolons are not used
- Terminator is `END-EXEC`

#### 4.1.3 AST Nodes

**New node: `CicsStatement`**

```csharp
public class CicsStatement : SubsystemStatementNode
{
    public string CommandName { get; set; }          // e.g., "READ", "WRITE"
    public List<CicsOption> Options { get; set; }    // e.g., FILE, INTO, RIDFLD
    public bool HasEndExec { get; set; }
}

public class CicsOption
{
    public string Keyword { get; set; }              // e.g., "FILE", "INTO"
    public ExpressionNode Value { get; set; }        // e.g., literal or variable
    public bool IsPositional { get; set; }
}
```

#### 4.1.4 Example

**COBOL source:**
```cobol
EXEC CICS READ FILE('CUSTFILE') INTO(WS-CUST-REC) RIDFLD(WS-CUST-ID) END-EXEC
```

**AST representation:**
```
CicsStatement
├── CommandName: "READ"
├── Options:
│   ├── { Keyword: "FILE", Value: Literal("CUSTFILE") }
│   ├── { Keyword: "INTO", Value: VariableReference("WS-CUST-REC") }
│   └── { Keyword: "RIDFLD", Value: VariableReference("WS-CUST-ID") }
└── HasEndExec: true
```

#### 4.1.5 Mutation Strategy Implications

| Original CICS Command | Mutation | Description |
|:---|:---|:---|
| `EXEC CICS READ` | `EXEC CICS READNEXT` | Change read type |
| `EXEC CICS WRITE` | `EXEC CICS REWRITE` | Change write operation |
| `EXEC CICS DELETE` | `EXEC CICS DELETE` (remove options) | Remove file specification |
| `EXEC CICS XCTL` | `EXEC CICS RETURN` | Change program control |
| `EXEC CICS SYNCPOINT` | Remove statement | Remove transaction boundary |

**CICS-Specific Mutation Strategy:**
```csharp
public class CicsCommandMutationStrategy : IMutationStrategy
{
    private static readonly Dictionary<string, string[]> CommandReplacements = 
        new Dictionary<string, string[]>
    {
        ["READ"] = new[] { "READNEXT", "READPREV" },
        ["WRITE"] = new[] { "REWRITE" },
        ["DELETE"] = new[] { "DELETE" },  // Remove options variant
        ["XCTL"] = new[] { "RETURN", "LINK" },
        ["START"] = new[] { "START" }     // Remove options variant
    };
    
    public bool CanApply(AstNode node) => node is CicsStatement;
    public List<Mutation> Apply(AstNode node) { /* ... */ }
    public MutationType Type => MutationType.CicsCommand;
}
```

---

### 4.2 IMS (Information Management System)

#### 4.2.1 Documentation Reference

IMS provides two primary access mechanisms:
- **DL/I (Data Language/I)** – Traditional hierarchical database manipulation
- **IMS SQL** – SQL-based access via embedded SQL statements

Documentation sources:
- *IMS Application Programming: Database Manager* (SC18-7809)
- IMS Version 14 Database Administration documentation

#### 4.2.2 Syntax Pattern

**DL/I Call Format**:

```cobol
CALL "CBLTDLI" USING function-code, pcb-name, i-o-area [, ssa...]
```

Or with the IMS SQL coprocessor:

```cobol
EXEC SQLIMS ... END-EXEC
```

#### 4.2.3 AST Nodes

**New nodes: `ImsDliStatement` and `ImsSqlStatement`**

```csharp
public class ImsDliStatement : SubsystemStatementNode
{
    public string FunctionCode { get; set; }           // e.g., "GU", "GN", "REPL", "ISRT"
    public ExpressionNode PcbName { get; set; }        // Program Communication Block
    public ExpressionNode IoArea { get; set; }         // I/O area
    public List<ExpressionNode> SsaList { get; set; }  // Segment Search Arguments
}

public class ImsSqlStatement : SubsystemStatementNode
{
    public string SqlText { get; set; }                // Raw SQL (SELECT, INSERT, etc.)
    public List<ExpressionNode> HostVariables { get; set; }
}
```

#### 4.2.4 Example

**DL/I call:**
```cobol
CALL "CBLTDLI" USING GU, CUSTPCB, WS-CUST-IO, WS-CUST-SSA.
```

**AST representation:**
```
ImsDliStatement
├── FunctionCode: "GU"          (Get Unique)
├── PcbName: VariableReference("CUSTPCB")
├── IoArea: VariableReference("WS-CUST-IO")
└── SsaList:
    └── [ VariableReference("WS-CUST-SSA") ]
```

#### 4.2.5 Mutation Strategy Implications

| Original DL/I Function | Mutation | Description |
|:---|:---|:---|
| `GU` (Get Unique) | `GN` (Get Next) | Change retrieval |
| `GN` (Get Next) | `GNP` (Get Next Within Parent) | Change navigation |
| `REPL` (Replace) | `DLET` (Delete) | Change data operation |
| `ISRT` (Insert) | `REPL` (Replace) | Change data operation |
| `DLET` (Delete) | `REPL` (Replace) | Change data operation |

**IMS-Specific Mutation Strategy:**
```csharp
public class ImsDliMutationStrategy : IMutationStrategy
{
    private static readonly Dictionary<string, string[]> DliReplacements = 
        new Dictionary<string, string[]>
    {
        ["GU"] = new[] { "GN" },
        ["GN"] = new[] { "GU", "GNP" },
        ["GNP"] = new[] { "GN" },
        ["REPL"] = new[] { "DLET", "ISRT" },
        ["ISRT"] = new[] { "REPL" },
        ["DLET"] = new[] { "REPL" }
    };
    
    public bool CanApply(AstNode node) => node is ImsDliStatement;
    public List<Mutation> Apply(AstNode node) { /* ... */ }
    public MutationType Type => MutationType.ImsDli;
}
```

---

### 4.3 ADABAS

#### 4.3.1 Documentation Reference

ADABAS direct call interface documented in:
- *Adabas Command Reference Guide* – Detailed command descriptions
- *Adabas Programming and Applications Guide*
- Adabas direct call COBOL examples

#### 4.3.2 Syntax Pattern

ADABAS direct calls in COBOL follow the format:

```cobol
CALL 'ADABAS' USING control-block [, format-buffer] [, record-buffer]
```

- `control-block` – Contains command code (A1, L1, ET, etc.) and parameters
- `format-buffer` – Describes field format for record operations
- `record-buffer` – Contains record data

Two interface types are supported:
- **ACB** – Traditional control block interface
- **ACBX** – Extended control block interface

#### 4.3.3 AST Nodes

**New node: `AdabasStatement`**

```csharp
public class AdabasStatement : SubsystemStatementNode
{
    public string InterfaceType { get; set; }          // "ACB" or "ACBX"
    public ExpressionNode ControlBlock { get; set; }   // Command/control block
    public ExpressionNode FormatBuffer { get; set; }   // Optional format buffer
    public ExpressionNode RecordBuffer { get; set; }   // Optional record buffer
    public string CommandCode { get; set; }            // Parsed from control block
}
```

#### 4.3.4 Example

**ADABAS call in COBOL:**
```cobol
CALL 'ADABAS' USING WS-CONTROL, WS-FORMAT, WS-RECORD.
```

**AST representation:**
```
AdabasStatement
├── InterfaceType: "ACB"
├── ControlBlock: VariableReference("WS-CONTROL")
├── FormatBuffer: VariableReference("WS-FORMAT")
├── RecordBuffer: VariableReference("WS-RECORD")
└── CommandCode: "L1"    (parsed from WS-CONTROL)
```

#### 4.3.5 Mutation Strategy Implications

| ADABAS Command | Mutation | Description |
|:---|:---|:---|
| `L1` (Read) | `L2` (Read Physical Sequential) | Change read mode |
| `L2` | `L3` (Read Logical Sequential) | Change read mode |
| `A1` (Update) | `E1` (Delete) | Change operation |
| `E1` (Delete) | `A1` (Update) | Change operation |
| `ET` (End Transaction) | Remove statement | Remove transaction commit |
| `BT` (Back Out) | Remove statement | Remove rollback |

**ADABAS Mutation Strategy:**
```csharp
public class AdabasMutationStrategy : IMutationStrategy
{
    private static readonly Dictionary<string, string[]> AdabasReplacements = 
        new Dictionary<string, string[]>
    {
        ["L1"] = new[] { "L2", "L3" },
        ["L2"] = new[] { "L1", "L3" },
        ["L3"] = new[] { "L1", "L2" },
        ["A1"] = new[] { "E1" },
        ["E1"] = new[] { "A1" },
        ["ET"] = new[] { "" },    // Remove transaction commit
        ["BT"] = new[] { "" }     // Remove rollback
    };
    
    public bool CanApply(AstNode node) => node is AdabasStatement;
    public List<Mutation> Apply(AstNode node) { /* ... */ }
    public MutationType Type => MutationType.AdabasCommand;
}
```

---

### 4.4 VSAM (Virtual Storage Access Method)

#### 4.4.1 Documentation Reference

VSAM is an access method for files on direct-access storage devices. Documentation sources:
- *z/OS DFSMS: Using Data Sets*
- *z/OS DFSMS Macro Instructions for Data Sets*
- *z/OS DFSMS: Access Method Services for Catalogs*

VSAM data set types:
| VSAM Type | COBOL Organization | Description |
|:---|:---|:---|
| Entry-Sequenced (ESDS) | SEQUENTIAL | Sequential file |
| Key-Sequenced (KSDS) | INDEXED | Indexed file |
| Relative-Record (RRDS) | RELATIVE | Relative file |

#### 4.4.2 Syntax Pattern

VSAM files are defined in the `FILE-CONTROL` paragraph of the `ENVIRONMENT DIVISION`:

```cobol
SELECT file-name ASSIGN TO assignment-name
       ORGANIZATION IS {SEQUENTIAL | INDEXED | RELATIVE}
       ACCESS MODE IS {SEQUENTIAL | RANDOM | DYNAMIC}
       FILE STATUS IS status-variable
       [VSAM-CODE IS vsam-status]
       [ALTERNATE RECORD KEY IS key-name]
```

File operations use standard COBOL I/O statements:
- `OPEN` / `CLOSE`
- `READ` (with `AT END` / `NOT AT END` clauses)
- `WRITE`
- `REWRITE`
- `DELETE`
- `START` (positioning in indexed files)

#### 4.4.3 AST Nodes

**New nodes: `VsamFileControlEntry` and `VsamFileOperation`**

```csharp
public class VsamFileControlEntry : AstNode
{
    public string FileName { get; set; }
    public string AssignmentName { get; set; }
    public string Organization { get; set; }           // "SEQUENTIAL", "INDEXED", "RELATIVE"
    public string AccessMode { get; set; }             // "SEQUENTIAL", "RANDOM", "DYNAMIC"
    public string FileStatusVariable { get; set; }
    public string VsamCodeVariable { get; set; }
    public List<VsamAlternateKey> AlternateKeys { get; set; }
}

public class VsamAlternateKey
{
    public string KeyName { get; set; }
    public bool IsUnique { get; set; }
}

public class VsamFileOperation : DataAccessStatementNode
{
    public string FileName { get; set; }
    public string Operation { get; set; }              // "READ", "WRITE", "REWRITE", "DELETE", "START"
    public bool IsConditional { get; set; }            // Has AT END / NOT AT END
    public ExpressionNode KeyExpression { get; set; }  // For RANDOM/DYNAMIC access
}
```

#### 4.4.4 Example

**VSAM file definition:**
```cobol
SELECT CUST-FILE ASSIGN TO CUSTVSAM
       ORGANIZATION IS INDEXED
       ACCESS MODE IS RANDOM
       RECORD KEY IS CUST-ID
       FILE STATUS IS WS-FS.
```

**VSAM operation:**
```cobol
READ CUST-FILE RECORD INTO WS-CUST-REC
     KEY IS WS-CUST-ID
     INVALID KEY DISPLAY 'Not Found'
     NOT INVALID KEY DISPLAY 'Found'.
```

#### 4.4.5 Mutation Strategy Implications

| Original | Mutation | Description |
|:---|:---|:---|
| `ACCESS MODE IS SEQUENTIAL` | `ACCESS MODE IS RANDOM` | Change access mode |
| `ACCESS MODE IS RANDOM` | `ACCESS MODE IS DYNAMIC` | Change access mode |
| `READ ... KEY IS` | Remove `KEY IS` clause | Remove key specification |
| `READ` | `READ ... INVALID KEY` | Add/remove error handling |
| `WRITE` | `REWRITE` | Change operation |
| `REWRITE` | `WRITE` | Change operation |

**VSAM Mutation Strategy:**
```csharp
public class VsamMutationStrategy : IMutationStrategy
{
    private static readonly string[] AccessModes = { "SEQUENTIAL", "RANDOM", "DYNAMIC" };
    private static readonly string[] Operations = { "READ", "WRITE", "REWRITE", "DELETE" };
    
    public bool CanApply(AstNode node) => node is VsamFileControlEntry || node is VsamFileOperation;
    public List<Mutation> Apply(AstNode node) 
    {
        var mutations = new List<Mutation>();
        if (node is VsamFileControlEntry entry)
        {
            // Mutate ACCESS MODE
            foreach (var mode in AccessModes.Where(m => m != entry.AccessMode))
            {
                mutations.Add(new Mutation
                {
                    OriginalNode = node,
                    MutatedNode = entry with { AccessMode = mode },
                    Type = MutationType.VsamAccessMode
                });
            }
        }
        // ... file operation mutations
        return mutations;
    }
    public MutationType Type => MutationType.VsamOperation;
}
```

---

## 5. Integrated Mutation Engine Interface

### 5.1 Extended Interface

```csharp
public interface IMutationEngine
{
    List<Mutation> GenerateMutations(CobolProgram program, MutationProfile profile);
    CobolProgram ApplyMutation(CobolProgram program, Mutation mutation);
    bool ValidateMutation(CobolProgram program, Mutation mutation);
}

public interface IMutationEngineV2 : IMutationEngine
{
    // Subsystem-aware mutation generation
    List<Mutation> GenerateMutations(
        CobolProgram program, 
        MutationProfile profile,
        SubsystemFilter filter          // [NEW] Filter by subsystem types
    );
    
    // AST analysis for subsystem dependencies
    SubsystemDependencyGraph AnalyzeDependencies(CobolProgram program);
    
    // Semantic validation with subsystem context
    bool ValidateSubsystemMutation(CobolProgram program, Mutation mutation);
}

public enum SubsystemType
{
    CoreCobol,
    Cics,
    Ims,
    Adabas,
    Vsam,
    Db2
}

public class SubsystemFilter
{
    public List<SubsystemType> Include { get; set; }
    public List<SubsystemType> Exclude { get; set; }
}

public class SubsystemDependencyGraph
{
    public Dictionary<string, List<string>> FileDependencies { get; set; }
    public Dictionary<string, List<string>> TransactionDependencies { get; set; }
    public Dictionary<string, List<string>> DatabaseDependencies { get; set; }
}
```

### 5.2 Extended Mutation Types

```csharp
public enum MutationType
{
    // Existing
    LogicalOperator,
    ArithmeticOperator,
    ConstantReplacement,
    ComplexExpression,
    
    // New subsystem mutations
    CicsCommand,           // EXEC CICS command mutations
    CicsOption,            // CICS option add/remove/modify
    ImsDli,                // DL/I function code mutations
    ImsSql,                // IMS SQL mutations
    AdabasCommand,         // ADABAS direct call command mutations
    AdabasParameter,       // ADABAS parameter mutations
    VsamAccessMode,        // VSAM access mode mutations
    VsamOperation,         // VSAM file operation mutations
    VsamErrorHandling,     // VSAM error handling mutations
    DatabaseTransaction    // Transaction boundary mutations
}
```

---

## 6. AST Parser Extension Requirements

### 6.1 Lexical Extensions

| Subsystem | New Tokens |
|:---|:---|
| CICS | `EXEC`, `CICS`, `END-EXEC`, command names |
| IMS | `CBLTDLI`, `CEETDLI`, function codes, `EXEC SQLIMS` |
| ADABAS | `ADABAS` (in CALL context) |
| VSAM | `VSAM-CODE`, `ALTERNATE RECORD KEY`, `RECORD KEY` |

### 6.2 Grammar Extensions

**CICS statement:**
```
cics_statement ::= 'EXEC' 'CICS' cics_command (cics_option)* 'END-EXEC'
cics_command   ::= IDENTIFIER
cics_option    ::= KEYWORD '(' expression ')' | KEYWORD expression
```

**IMS DL/I call:**
```
ims_dli_call ::= 'CALL' STRING_LITERAL ('CBLTDLI' | 'CEETDLI') 'USING' 
                 expression (',' expression)*
```

**ADABAS call:**
```
adabas_call ::= 'CALL' STRING_LITERAL ('ADABAS') 'USING' 
                expression (',' expression)*
```

**VSAM file operation:**
```
vsam_operation ::= ('READ' | 'WRITE' | 'REWRITE' | 'DELETE') file_name
                   ('RECORD' 'INTO' expression)?
                   ('KEY' 'IS' expression)?
                   (conditional_clause)?
```

### 6.3 Semantic Analysis Extensions

| Subsystem | New Semantic Checks |
|:---|:---|
| CICS | Validate command name against known CICS verbs; validate options per command |
| IMS | Validate function code; validate PCB exists in DATA DIVISION |
| ADABAS | Validate command code in control block; validate buffer references |
| VSAM | Validate ORGANIZATION/ACCESS compatibility; validate key references |

---

## 7. Implementation Roadmap

### Phase 1: AST Infrastructure (Weeks 1-2)
- Define new AST node classes
- Update parser grammar
- Implement lexical extensions

### Phase 2: CICS Integration (Weeks 3-4)
- Implement `CicsStatement` parsing
- Implement `CicsCommandMutationStrategy`
- Unit tests with sample CICS programs

### Phase 3: IMS Integration (Weeks 5-6)
- Implement `ImsDliStatement` and `ImsSqlStatement` parsing
- Implement `ImsDliMutationStrategy`
- Unit tests with DL/I samples

### Phase 4: ADABAS Integration (Weeks 7-8)
- Implement `AdabasStatement` parsing
- Implement `AdabasMutationStrategy`
- Unit tests with ADABAS direct call samples

### Phase 5: VSAM Integration (Weeks 9-10)
- Implement `VsamFileControlEntry` and `VsamFileOperation` parsing
- Implement `VsamMutationStrategy`
- Unit tests with VSAM file definitions

### Phase 6: Integration & Validation (Weeks 11-12)
- Integration testing with real-world programs
- Performance benchmarking
- Documentation and examples

---

## 8. References

### IBM Enterprise COBOL for z/OS
1. *Enterprise COBOL for z/OS Language Reference* (SC27-8713-04)
2. *Enterprise COBOL for z/OS Programming Guide* (SC27-8714-04)

### CICS
3. *CICS Application Programming Reference* (SC34-7159)
4. *CICS Application Programming Guide* (SC34-7158)
5. *API (EXEC CICS) Reference*

### IMS
6. *IMS Application Programming: Database Manager* (SC18-7809)
7. *IMS Database Administration Guide* (ADB, SC18-7806)

### ADABAS
8. *Adabas Command Reference Guide*
9. *Adabas Programming and Applications Guide*
10. *Adabas Calling Procedure Documentation*

### VSAM
11. *z/OS DFSMS: Using Data Sets*
12. *z/OS DFSMS Macro Instructions for Data Sets*
13. *z/OS DFSMS: Access Method Services for Catalogs*

---

## 9. Appendix: Mutation Examples

### CICS Mutation Example

**Original:**
```cobol
EXEC CICS READ FILE('CUSTFILE') INTO(WS-CUST) RIDFLD(WS-ID) END-EXEC
```

**Mutant:**
```cobol
EXEC CICS READNEXT FILE('CUSTFILE') INTO(WS-CUST) RIDFLD(WS-ID) END-EXEC
```

### IMS DL/I Mutation Example

**Original:**
```cobol
CALL "CBLTDLI" USING GU, CUSTPCB, WS-CUST-IO, WS-CUST-SSA.
```

**Mutant:**
```cobol
CALL "CBLTDLI" USING GN, CUSTPCB, WS-CUST-IO, WS-CUST-SSA.
```

### ADABAS Mutation Example

**Original:**
```cobol
CALL 'ADABAS' USING WS-CONTROL, WS-FORMAT, WS-RECORD.
```

**Mutant:**
```cobol
CALL 'ADABAS' USING WS-CONTROL-MUT, WS-FORMAT, WS-RECORD.  // Command changed L1→L2
```

### VSAM Mutation Example

**Original:**
```cobol
SELECT CUST-FILE ASSIGN TO CUSTVSAM
       ORGANIZATION IS INDEXED
       ACCESS MODE IS SEQUENTIAL
       RECORD KEY IS CUST-ID.
```

**Mutant:**
```cobol
SELECT CUST-FILE ASSIGN TO CUSTVSAM
       ORGANIZATION IS INDEXED
       ACCESS MODE IS RANDOM        // MUTATED
       RECORD KEY IS CUST-ID.
```

---

## 10. Conclusion

This RFC establishes a comprehensive framework for expanding the COBOL AST parser to support mutation testing across the IBM z/OS enterprise ecosystem. By adding semantic awareness of CICS, IMS, ADABAS, and VSAM subsystems, the mutation engine can generate meaningful mutants that detect faults in:

- Transaction processing logic (CICS)
- Hierarchical database operations (IMS)
- Direct database calls (ADABAS)  
- File access patterns (VSAM)

The phased implementation approach ensures incremental delivery with validation at each stage. The extended AST nodes and mutation strategies integrate seamlessly with the existing `IMutationEngine` interface while providing subsystem-specific capabilities required for enterprise COBOL applications.