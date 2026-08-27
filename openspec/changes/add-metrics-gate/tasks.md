## 1. Analyzer configuration

- [x] 1.1 Add `.editorconfig` at repo root enabling CA1502 and CA1505 at error severity
- [x] 1.2 Add `CodeMetricsConfig.txt` with `CA1502: 10` and `CA1505: 30`
- [x] 1.3 Add root `Directory.Build.props` referencing `CodeMetricsConfig.txt` as `AdditionalFiles` via `$(MSBuildThisFileDirectory)`
- [x] 1.4 Run `dotnet build` and triage CA1502/CA1505 violations (fix or suppress)

## 2. Duplication gate

- [x] 2.1 Add a `duplication` target to the `Makefile` running jscpd on `src/` with `--threshold 10`
- [x] 2.2 Add `duplication` to `.PHONY` and the `help` text
- [x] 2.3 Install jscpd locally and verify `make duplication` passes on the current codebase

## 3. Quality gate and CI wiring

- [x] 3.1 Add `duplication` to the `make quality-gate` target
- [x] 3.2 Update `.github/workflows/ci.yml` to install jscpd and run the quality gate
- [x] 3.3 Verify CI is green (or confirm locally via `make quality-gate`)
