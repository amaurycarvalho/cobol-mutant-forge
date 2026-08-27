## Context

`CobolMutantForge` mutates COBOL source by walking a generic `AstNode` tree (`Kind`/`Line`/`Column`/`Text`/`Children`) produced by `TypeCobolParserAdapter`, a documented line-scanned fallback parser (TypeCobol is not yet consumable from NuGet). Mutation strategies implement `IMutationStrategy` (`MutationType`, `OperationType`, `Apply(CobolProgram)`) and emit line-scoped `Mutation` records (`Original` → `Mutated` + `Line`). `MutationEngine` deduplicates, validates, and assigns ids, gating each strategy by `OperationType` mapped to `MutationProfile` flags.

RFC-001 proposes a full z/OS subsystem expansion (CICS, IMS DL/I, ADABAS, VSAM), but assumes a class-based AST hierarchy, an AST-node rewrite model, a grammar-based parser, and a `SubsystemFilter`/`IMutationEngineV2` that do not exist here. This design adapts the RFC to the actual architecture.

## Goals / Non-Goals

**Goals:**

- Extend the existing parser to emit discoverable `AstNode`s for CICS, IMS DL/I, ADABAS, and VSAM constructs.
- Add subsystem mutation strategies that follow the current `IMutationStrategy` + line-scoped `Mutation` model.
- Extend `MutationType`, `OperationType`, and `MutationProfile` minimally and consistently with existing conventions.
- Wire the new strategies into `MutationEngine` and gate them by profile.

**Non-Goals:**

- No class-based AST hierarchy (`ExpressionNode`, `SubsystemStatementNode`, etc.). The generic `AstNode` with `Kind` strings is retained.
- No real grammar/lexer rewrite; the heuristic scanner is extended, not replaced.
- No multi-line `EXEC CICS ... END-EXEC` handling (single-line only).
- No ADABAS command-code mutation (requires data-flow analysis of a runtime control block).
- No `SubsystemFilter`, `SubsystemDependencyGraph`, `SubsystemType`, or `IMutationEngineV2`.
- No statement deletion unless the whole statement occupies a single source line.

## Decisions

### 1. Keep the generic `AstNode`; add `Kind` discriminators (not a class hierarchy)

The parser and all strategies key off `node.Kind` (e.g., `"LogicalOperator"`). Introducing the RFC's OO hierarchy would force a rewrite of traversal, strategies, and tests for no immediate gain. New constructs become new `Kind` values with options carried in `Children`.

**Alternatives considered:** (a) RFC class hierarchy — rejected, high churn, no consumer benefit yet; (b) parallel tree type — rejected, doubles the traversal surface.

### 2. New AST node kinds and shape

| Kind | `Text` | `Children` |
|:---|:---|:---|
| `CicsStatement` | command name (`READ`, `WRITE`, …) | `CicsOption` nodes |
| `CicsOption` | option keyword (`FILE`, `INTO`, `RIDFLD`, …) | one value node (`Literal` / `VariableReference`) |
| `ImsDliStatement` | function code (`GU`, `GN`, …) | using-argument `VariableReference` nodes |
| `AdabasStatement` | `"ADABAS"` | using-argument `VariableReference` nodes (control, format, record buffers) |
| `VsamFileControlEntry` | file name | clause nodes (`Organization`, `AccessMode`, `RecordKey`) |
| `VsamFileOperation` | operation (`READ`, `WRITE`, `REWRITE`, `DELETE`, `START`) | file name + `KeyIs` nodes |

Parsing stays line-based. `EXEC CICS` is recognized only when `END-EXEC` appears on the same line; `CALL "CBLTDLI"/"CEETDLI" USING …` and `CALL 'ADABAS' USING …` are recognized by the CALL target literal; `SELECT … ASSIGN …` and `READ/WRITE/REWRITE/DELETE/START …` are recognized by leading verb. New keywords are added to `KnownStatementKeywords` so the scanner stops flagging them as "possibly unsupported".

**Alternatives considered:** multi-line `EXEC CICS` state machine — deferred (user decision: single-line first).

### 3. Fine-grained `MutationType` values (existing convention)

Follow the `AndToOr` / `AddToSubtract` naming (`SourceToTarget`). Subsystem additions:

- CICS: `CicsReadToReadnext`, `CicsReadToReadprev`, `CicsWriteToRewrite`, `CicsXctlToReturn`, `CicsXctlToLink`, `CicsSynpointRemove`.
- IMS DL/I: `ImsGuToGn`, `ImsGnToGu`, `ImsGnToGnp`, `ImsGnpToGn`, `ImsReplToDlet`, `ImsReplToIsrt`, `ImsIsrtToRepl`, `ImsDletToRepl`.
- ADABAS: `AdabasBufferRemoval` (remove an optional format/record buffer argument).
- VSAM: `VsamSequentialToRandom`, `VsamSequentialToDynamic`, `VsamRandomToSequential`, `VsamRandomToDynamic`, `VsamDynamicToSequential`, `VsamDynamicToRandom`, `VsamWriteToRewrite`, `VsamRewriteToWrite`.

**Alternatives considered:** coarse category enums (`CicsCommand`, `ImsDli`) per RFC — rejected; inconsistent with the existing enum (user decision: current style).

### 4. Profile gating via per-subsystem flags (no `SubsystemFilter`)

Add `OperationType` values `Cics`, `Ims`, `Adabas`, `Vsam`, and matching boolean flags on `MutationProfile` (`Cics`, `Ims`, `Adabas`, `Vsam`). `MutationEngine.IsEnabled` maps each new `OperationType` to its flag. Flags default to `false` in `low`/`medium` and `true` in `high`. `MutationFlagsDto` and `DefaultConfigFactory` gain the four flags (JSON deserialization leaves absent fields `false`, so existing configs remain valid).

**Alternatives considered:** a separate `SubsystemFilter` dimension per RFC — rejected (user decision: least impact); reusing existing `OperationType` categories — rejected, subsystems are a distinct axis.

### 5. Statement removal is line-scoped

`Mutation` already supports `Mutated = string.Empty` (`RemoveNot`). `SYNCPOINT`/`ET`/`BT` removal is emitted only when the statement occupies one line, matching `ValidateMutation`'s line containment check.

### 6. ADABAS reduced scope

Only the `CALL 'ADABAS' USING …` argument list is mutated (drop optional `format-buffer`/`record-buffer`). Command-code mutation is deferred; the code (e.g., `L1`) is written into the control block at runtime, not present in the `CALL`.

## Risks / Trade-offs

- **Heuristic parser may mis-identify subsystem tokens** (e.g., a variable named `READ` in a non-I/O context) → Mitigation: recognize by structural position (leading verb + known trailing clauses), not bare keyword; unit tests with real sample programs.
- **Line-scoped model limits multi-line constructs** → Mitigation: single-line scope is explicit; multi-line `EXEC CICS` is a tracked follow-up.
- **Enum growth** (fine-grained types) → Mitigation: acceptable; grouped per subsystem in code and docs; names are self-describing.
- **ADABAS coverage is thin** (only buffer removal) → Mitigation: documented as reduced scope; command-code mutation is a future data-flow item.
- **Profile semantics change** (`high` now also enables subsystems) → Mitigation: flags default off in `low`/`medium`; no change to existing low/medium output.
- **ADABAS/IMS false positives from unrelated `CALL` literals** → Mitigation: match exact target literals (`ADABAS`, `CBLTDLI`, `CEETDLI`).
