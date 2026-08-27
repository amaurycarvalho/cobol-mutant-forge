## Context

The PRD (sections 5.3.3, 5.3.4) defines the published objects for CICS import: mutated `.cbl`, `manifest.json`, `mutations-report.json`, and a `.zip` package. The exporter realizes `IExportPlugin` declared in the domain.

## Goals / Non-Goals

**Goals:**
- A `MutantPackageExporter` producing both `zip` and `folder` outputs.
- Exact manifest structure from the PRD.
- A complete `mutations-report.json`.

**Non-Goals:**
- CICS Bundle (`.zip`) consumption/validation of structure (a reference-only concern in MVP).
- Direct deployment to CICS regions (out of scope).

## Decisions

- **Single exporter, two output modes** — `zip` and `folder` share the same file-assembly logic; `zip` just archives the folder output. Keeps one code path.
- **Manifest/report as plain JSON via System.Text.Json** — matches the PRD and avoids new dependencies.
- **`baseProgramHash`** — derived from the original source to support later change detection (PRD manifest field).
- **Id scheme** — `MUT-<seq>-<program>-<index>` mirrors the PRD example `MUT-001-PAYMENT-INT-001`.

## Risks / Trade-offs

- [ZIP structure compatibility with CICS Explorer is not verified in MVP] → Document the structure; treat as reference-only until user validation.
- [Large output sets] → Stream files into the archive rather than buffering all content in memory.

## Migration Plan

None — additive. Consumed by `cli-commands`.
