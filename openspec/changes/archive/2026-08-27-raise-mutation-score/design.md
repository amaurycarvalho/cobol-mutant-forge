## Context

The Stryker.NET mutation run (`make mutation`) reports a final score of 14.73%: 118 mutants killed, 76 "Survived" (covered but weakly asserted), 607 "NoCoverage" (never executed by tests), 36 "CompileError", and 192 "Ignored". The score denominator is 801 (detectable mutants = killed + survived + no-coverage). Reaching 80% requires 641 killed, i.e. killing 523 of the 683 survivors.

The config `tests/CobolMutantForge.Tests/stryker-config.json` declares `mutate` scoped to Domain and Application, but `make mutation` invokes `dotnet-stryker --solution` from the repo root and does not find that config, so Stryker mutates all four projects (Domain, Application, Infrastructure, CLI). This change deliberately follows Route A: raise the score over the full solution scope without narrowing the mutation scope.

## Goals / Non-Goals

**Goals:**

- Kill at least 523 surviving mutants so the full-solution mutation score exceeds 80%.
- Strengthen existing tests for the 76 "Survived" mutants (cheapest wins).
- Add focused tests for the 607 "NoCoverage" mutants, ordered by ROI.
- Keep the change test-only: no production code or project-file modifications.

**Non-Goals:**

- Narrowing the Stryker `mutate` scope (Route B) to inflate the score.
- Fixing or reducing the 36 "CompileError" and 192 "Ignored" mutants (excluded from the score and require production changes).
- Refactoring production code for testability; if a test is hard to write, prefer asserting observable behavior through public APIs rather than changing code.

## Decisions

### Decision 1: Follow Route A (full-scope) and prioritize by mutant category

Killing 523 of 683 survivors requires sequencing. The two survivor categories have very different cost:

- **Survived (76)**: the code is already exercised; only assertions are missing. Strengthen assertions first.
- **NoCoverage (607)**: new tests are needed. Order by (mutants per file) and (effort per mutant).

Priority order by ROI:

1. Strengthen assertions for the 76 Survived mutants (parsers, CLI commands, mutation profile).
2. Domain + Application (the project's declared core): entities, value objects, aggregates, ast, interfaces, services, use cases, configuration DTOs — small POCOs with high mutant density per line.
3. Infrastructure data layers (serialization, parsers, plugins, exporters, configuration) — medium effort, many mutants.
4. Infrastructure mutators (engine + strategies) — highest volume of no-coverage; requires exercising all profile-gating branches.

### Decision 2: Test patterns must follow existing conventions

Tests SHALL use xUnit `[Fact]`/`[Theory]` in the existing namespaces, follow the xUnit1051 rule (use `Assert.All`, no in-loop assertions), and reuse existing constants/helpers (e.g., `CreateProgram` in `MutationEngineTests`). No new test framework or external dependency is introduced.

### Decision 3: Assert observable behavior, not internals

For each mutant category, the assertion strategy is:

- String/constant mutations → assert exact output text or diagnostic message, not "exists".
- Equality/logical/null-coalescing mutations in entities/value objects → assert equality/hash-code and null-guard branches directly.
- Statement/block-removal mutations → assert the observable side effect (collection content, result value) so removal is detected.
- Profile-gating branches in the mutation engine/strategies → assert per-profile mutation output for every `OperationType`.

### Decision 4: Track progress against the mutation report

Verification is `make mutation`. Because Stryker reruns the whole solution, each phase can be validated incrementally against the module-level kill counts captured from the report. The per-module survivor baseline is:

| Area | survivors |
|---|---|
| Infrastructure/Parsers | 137 |
| Infrastructure/Mutators | 130 |
| Infrastructure/Serialization | 105 |
| Infrastructure/Exporters | 78 |
| Infrastructure/Plugins | 74 |
| CLI/Commands | 42 |
| Domain/Entities | 33 |
| Application/UseCases | 16 |
| Application/Configuration | 20 |
| Application/Services | 12 |
| Domain/ValueObjects | 15 |
| Domain/Aggregates | 7 |
| Infrastructure/Configuration | 7 |
| Domain/Interfaces | 3 |
| Domain/Ast | 2 |

## Risks / Trade-offs

- [Some mutants (CompileError/Ignored) cannot be killed by tests] → They are excluded from the score; the plan does not target them. If the score still falls short after all planned tests, revisit whether scope is acceptable, but do not change production code silently.
- [Test suite growth may slow CI/mutation runs] → Keep tests focused on observable behavior; prefer a few high-value theories over exhaustive duplication.
- [Stryker timeout (`timeout-ms: 10000`)] → New tests must remain fast; avoid I/O-heavy tests and use in-memory fixtures consistent with existing tests.
- [Weak assertions can still leave "Survived" mutants] → Prefer asserting concrete output values and edge cases (null, empty, boundary) rather than `NotEmpty`/`True` style checks.
