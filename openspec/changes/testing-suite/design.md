## Context

The PRD (sections 7.1–7.4) specifies xUnit v3.2.2, MTP v1 BDD, and Stryker.NET. The tool's own logic (mutation engine, profiles, import) is now implemented and must be proven. This change adds the test projects and configuration without modifying production code.

## Goals / Non-Goals

**Goals:**
- Unit tests for domain invariants and the mutation engine.
- BDD scenarios mirroring the PRD's Gherkin examples.
- A Stryker config that mutates the tool itself to validate its own test suite.

**Non-Goals:**
- End-to-end mainframe/CICS validation (out of MVP scope).
- Testing the Test Accelerator stub beyond its inertness.

## Decisions

- **xUnit v3.2.2** — mandated by the PRD; the test project already references it from `bootstrap-project`.
- **MTP v1 (`--test-runner mtp`)** — matches the PRD and the existing Makefile convention.
- **Stryker scope** — mutate `Domain` and `Application` only, excluding test code, per the PRD's `stryker-config.json`.
- **xUnit1051 avoidance** — use `Assert.All`/`[Theory]` instead of assertions in `foreach` loops, per the PRD's own guidance.

## Risks / Trade-offs

- [Stryker runs can be slow] → Kept out of the CI fast path; run manually via `make mutation`.
- [Coverage >80% is a stretch for a scaffolded tool] → The threshold is aspirational; the CI gate enforces it only after the test suite matures.

## Migration Plan

None — additive.
