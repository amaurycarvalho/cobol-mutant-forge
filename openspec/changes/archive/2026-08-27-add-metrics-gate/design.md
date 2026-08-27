## Context

The repository already has a quality gate (`make quality-gate` → lint, test, coverage-check, metrics, security) and a CI workflow (`.github/workflows/ci.yml`) that runs it on `ubuntu-latest`. The Makefile's `metrics` target today only reports raw lines-of-code per project via `find | wc -l`; it does not gate anything.

The project wants to enforce three additional metrics as *blocking* checks: cyclomatic complexity (≤10), maintainability index (≥30), and code duplication (≤10%). A tooling evaluation ruled out the most obvious mechanisms:

- `Microsoft.CodeAnalysis.Metrics` + `msbuild /t:Metrics`: ships a .NET Framework 4.7.2 `Metrics.exe` (Windows-only), so it cannot run on `ubuntu-latest`.
- `CodeCheckerCLI`: single-author toy (2 stars, 333 downloads), no percentage output, library rather than a CLI.
- `knots`/`knotsjs`: a JavaScript call-graph tool — irrelevant to C# and to these metrics.

The remaining mechanisms that work cross-platform on Linux CI are the built-in .NET code-quality analyzers (CA1502/CA1505) for complexity/maintainability, and jscpd for duplication.

## Goals / Non-Goals

**Goals:**

- Enforce cyclomatic complexity ≤10 at build time via CA1502.
- Enforce maintainability index ≥30 at build time via CA1505.
- Gate code duplication at 10% via jscpd in a `make duplication` target.
- Wire the duplication check into `make quality-gate` and `ci.yml`.

**Non-Goals:**

- Per-function source-lines-of-code (>80) enforcement. No mature cross-platform tool does this for C#; CA1502/CA1505 already punish overly long methods via the maintainability index. Revisit as a custom Roslyn analyzer in a later change if still needed.
- Mutation-score thresholding (Stryker already exists as a manual `make mutation`; adding a numeric gate is out of scope here).
- Coverage threshold changes (existing `COVERAGE_THRESHOLD=90` stays; the "<85%" table value is not adopted here).
- SonarQube/Docker-based stacks (excluded by the `project-metadata` constitution).

## Decisions

**D1 — Use built-in Roslyn analyzers instead of `Microsoft.CodeAnalysis.Metrics`.**
CA1502 and CA1505 ship with the .NET SDK (disabled by default), run during `dotnet build`, and work identically on Linux and Windows. This replaces the `msbuild /t:Metrics` XML-report approach entirely.
*Alternatives considered:* `Microsoft.CodeAnalysis.Metrics` (Windows-only .NET Framework exe — rejected); a custom post-build XML parser (unnecessary once analyzers are used).

**D2 — Configure thresholds via `CodeMetricsConfig.txt` + `AdditionalFiles`.**
The documented mechanism for both CA1502 and CA1505 thresholds is a text file marked as `AdditionalFiles`:
```
CA1502: 10
CA1505: 30
```
*Alternatives considered:* `.editorconfig` numeric keys (`dotnet_code_quality.CA1502.method_complexity_threshold`) — works, but the `CodeMetricsConfig.txt` route is the stable, documented one and keeps thresholds in a single file.

**D3 — Enable the rules at `error` severity via `.editorconfig`.**
The rules are off by default; a root `.editorconfig` with `dotnet_diagnostic.CA1502.severity = error` (and the same for CA1505) turns them into blocking violations. `CodeMetricsConfig.txt` alone only sets the threshold and would otherwise silently do nothing.
*Alternatives considered:* `warning` + `TreatWarningsAsErrors` (more moving parts for the same effect — rejected).

**D4 — Share the wiring with a root `Directory.Build.props`.**
The `AdditionalFiles` reference needs a stable path, so it uses `$(MSBuildThisFileDirectory)CodeMetricsConfig.txt` in a repo-root `Directory.Build.props`, inherited by all projects. `.editorconfig` at repo root is picked up automatically by directory hierarchy.
*Alternatives considered:* per-project `<AdditionalFiles>` in all five `.csproj` files (duplication — rejected).

**D5 — Use jscpd (Rust v5) for duplication with `--threshold 10`.**
jscpd is mature (6.1k stars, 3.2M weekly npm downloads), uses Rabin-Karp token-stream clone detection, supports C# among 223 formats, ships a self-contained Linux binary (no Node runtime), and `--threshold 10` exits non-zero when duplication exceeds 10% — matching the blocking requirement.
*Alternatives considered:* PMD CPD (JRE dependency — rejected for a .NET-only CI); CodeCheckerCLI (rejected, see Context); a custom parser (unnecessary).

**D6 — Run jscpd on `src/` only, ignoring build artifacts.**
`make duplication` runs `jscpd src --format csharp --threshold 10 --ignore-pattern "**/obj/**" --ignore-pattern "**/bin/**" --reporters console,json`. Scoping to `src/` avoids flagging test fixture duplication that is not production code.

## Risks / Trade-offs

- **[New analyzer errors could break existing builds]** → This is intended; violations are fixed or suppressed with `#pragma warning disable` when justified. Run `dotnet build` first and triage before enabling in CI.
- **[Test project may have legitimately long/complex methods]** → The root `.editorconfig` applies to all projects. If tests trip CA1502/CA1505, either refactor or add a scoped `tests/**` section setting severity to `none`.
- **[jscpd false positives (boilerplate, generated code)]** → Mitigate with `--min-tokens`/`--min-lines` tuning and `--ignore-pattern`; generated/`obj`/`bin` are already excluded.
- **[New tool-install step in CI]** → jscpd needs a one-time install (npm, cargo, brew, or the `kucherenko/jscpd` GitHub Action). Pin the version to avoid drift.
- **[CA1505 MI formula may differ slightly from Visual Studio]** → Acceptable; the analyzer is the source of truth for the gate.
- **[`SourceLines`/SLOC still unmeasured per function]** → Accepted non-goal; CA1505's MI (a function of LOC) provides indirect coverage.

## Migration Plan

1. Add `.editorconfig`, `CodeMetricsConfig.txt`, and `Directory.Build.props` at repo root.
2. Run `dotnet build`; fix or suppress any CA1502/CA1505 violations.
3. Add the `duplication` target and wire it into `make quality-gate`.
4. Install jscpd locally and verify `make duplication` passes on the current codebase.
5. Update `ci.yml` to install jscpd and run `make quality-gate`.
6. Verify CI is green; rollback is simply reverting the `.editorconfig`/`Makefile`/`ci.yml` changes (no data migration).

## Open Questions

- Should CA1502/CA1505 apply to the test project, or be scoped to `src/` only?
- Which jscpd install method to use in CI: npm, cargo, brew, or the `kucherenko/jscpd` GitHub Action? (Recommend npm with a pinned version for `ubuntu-latest`.)
