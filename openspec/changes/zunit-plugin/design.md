## Context

The PRD (section 5.3) defines the exact artifacts ZUnit exports from CICS and how CobolMutantForge consumes them. The domain already declares `IImportPlugin`/`IExportPlugin` and the entities (`CobolProgram`, `TestCase`, `MutantPackage`), and the parser adapter produces ASTs. This change implements the importer side of the ZUnit plugin and the plugin base abstraction.

## Goals / Non-Goals

**Goals:**
- A `ZUnitPlugin` that consumes the four artifact kinds and assembles a `ZUnitImportResult`.
- A reusable `PluginBase` and a `TestAcceleratorPlugin` stub.
- Graceful handling of malformed or missing artifacts via warnings.

**Non-Goals:**
- Export/package generation (owned by `export-plugin`).
- Real Test Accelerator integration (v2.0).
- Executing or validating tests (out of MVP scope).

## Decisions

- **PluginBase + IImportPlugin/IExportPlugin contracts** — mirrors the PRD structure and keeps the CLI's `plugin list` command uniform.
- **`ZUnitImportResult` as the aggregate result** — carries programs, test cases, config, copybooks, warnings, and validity; matches the PRD's consumption structure exactly.
- **Tolerant parsing** — malformed artifacts add warnings rather than aborting, since ZUnit's XML format is undocumented (PRD risk) and the tool should still produce partial results.
- **Separate serializers** — `ZUnitXmlParser` and `JsonConfigSerializer` remain distinct, matching the PRD's Infrastructure layout.

## Risks / Trade-offs

- [ZUnit XML format is undocumented (PRD: high probability)] → Reverse-engineer from sample exports; keep the parser decoupled so it can be corrected without touching other layers.
- [COPYBOOK resolution can be ambiguous] → Resolve from the configured copybook directory; warn on missing references.

## Migration Plan

None — additive. Depends on `domain-model` and `type-cobol-parser`.
