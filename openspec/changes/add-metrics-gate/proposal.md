## Why

The quality gate currently enforces only formatting, tests, coverage, and a raw lines-of-code count. Three of the project's target metrics — cyclomatic complexity, maintainability index, and code duplication — have no automated enforcement, so complex or duplicated code can merge unnoticed. A tooling evaluation ruled out the obvious candidates (`Microsoft.CodeAnalysis.Metrics` is Windows-only; `CodeCheckerCLI` and `knots` are unsuitable), leaving the built-in .NET analyzers (CA1502/CA1505) plus jscpd as the only mechanisms that work on the `ubuntu-latest` CI.

## What Changes

- Add a `.editorconfig` that enables the built-in code-quality analyzers CA1502 (cyclomatic complexity) and CA1505 (maintainability index) as blocking build violations.
- Add a `CodeMetricsConfig.txt` (`CA1502: 10`, `CA1505: 30`) and wire it as `AdditionalFiles` so the rules fire at the intended thresholds.
- Add a `make duplication` target that runs jscpd with a 10% duplication threshold and fails the build when exceeded.
- Wire the duplication check into the existing quality gate and the `ci.yml` workflow.
- Keep `make metrics` (LOC report) as-is; explicitly treat per-function source-lines-of-code (>80) as a non-goal for this change.

## Capabilities

### New Capabilities

- `code-metrics-gate`: automated, build-time enforcement of cyclomatic complexity (CA1502) and maintainability index (CA1505), plus a code-duplication gate (jscpd) integrated into the Makefile and CI quality gate.

### Modified Capabilities

- `project-metadata`: the CI quality gate and Makefile target surface now include the metrics/duplication checks.

## Impact

- New files: `.editorconfig`, `CodeMetricsConfig.txt`.
- Project files: add `<AdditionalFiles Include="CodeMetricsConfig.txt" />` to the source projects.
- `Makefile`: new `duplication` target; quality gate invocation updated.
- `.github/workflows/ci.yml`: install jscpd and run the duplication check.
- New external tool dependency: `jscpd` (self-contained Rust binary; installed via npm, cargo, brew, or a GitHub Action).
- No API, runtime, or public-surface changes.
