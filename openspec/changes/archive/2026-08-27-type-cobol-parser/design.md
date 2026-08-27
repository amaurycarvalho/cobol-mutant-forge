## Context

The PRD technology stack mandates TypeCobol (latest stable from GitHub) as the COBOL parser. The domain already declares `ICobolParser` and a minimal `AstNode` abstraction. This change implements the Infrastructure-side adapter that bridges TypeCobol's native tree and the domain's AST.

## Goals / Non-Goals

**Goals:**
- A working `TypeCobolParserAdapter` implementing `ICobolParser`.
- Reliable operator-node discovery (logical + arithmetic).
- Correct exclusion of comments and literals from mutation candidates.

**Non-Goals:**
- Implementing mutation strategies themselves (owned by `mutation-engine`).
- Supporting every COBOL dialect — the MVP targets common constructs and flags the rest (PRD risk section).
- Parsing cache (a PRD "future feature").

## Decisions

- **Adapter pattern** — TypeCobol is a concrete dependency kept out of the domain. The adapter lives in Infrastructure so it can be swapped (e.g., an ANTLR fallback) without touching callers.
- **Minimal `AstNode` contract in the domain** — the adapter maps TypeCobol nodes onto a small node model (type, operator kind, line, source span) rather than leaking TypeCobol types upward.
- **Diagnostics as a first-class result** — a parse result carries both the AST and a diagnostics list, matching `ZUnitImportResult.Warnings` style and enabling graceful degradation.
- **Comment/literal exclusion at parse time** — nodes are only emitted for real operator tokens, protecting the mutation engine from invalid mutants.

## Risks / Trade-offs

- [TypeCobol may not cover all COBOL constructs (PRD: high impact)] → Validate against representative programs; keep the adapter surface narrow and document the fallback to ANTLR.
- [TypeCobol's NuGet/GitHub packaging may be unstable] → Pin to a specific release; wrap the dependency so it can be replaced.

## Migration Plan

None — additive. Existing source has no parser; the domain `ICobolParser` port is implemented for the first time.
