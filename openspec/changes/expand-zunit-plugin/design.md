## Context

The ZUnit plugin (`ZUnitPlugin : PluginBase, IImportPlugin`) imports ZUnit exports tolerantly: `.xml` via `ZUnitXmlParser`, `.bzucfg` via `ZUnitConfigParser`, `.cbl` via `TypeCobolParserAdapter`, and COPYBOOKS by filename discovery. Results are `record` types with `init` properties and `IReadOnlyList<>` collections. `ZUnitImportResult : ImportResult` carries `Programs`, `TestCases`, `Config`, `Copybooks`, `Warnings`, `IsValid`. Export is `MutantPackageExporter : IExportPlugin`, emitting `.cbl` mutants, `manifest.json` (`ManifestDto`), and `mutations-report.json`. The house style is tolerant parsing (never throw on malformed input; record warnings) and no new external dependencies.

RFC-002 proposes a larger integration, but parts of it (`.pb` binary datasets, DTR REST, an Eclipse/IDz preprocessor extension point, live CICS security enforcement) belong to a z/OS runtime a cross-platform CLI does not touch. This design scopes the change to the file-format expansions that fit the codebase and defers the runtime coupling behind inert stubs.

## Goals / Non-Goals

**Goals:**

- Extend import with tolerant parsers for `.xsd`, recorded runtime data (`batchrun`), AZUGEN generation configs, and runner results.
- Read CICS context from a dedicated `cics-context.json`.
- Extend export with per-mutant `test-data.xml`, generated `test-data.xsd`, and `coverage-map.json`.
- Extend `ManifestDto` with optional `cicsContext`, `recordedEntries`, and `schemaVersion`.
- Preserve framework-neutrality so TAZ (RFC-003) can reuse the new structures.

**Non-Goals:**

- No parsing of `.pb` playback datasets (binary MVS format) — stub only.
- No DTR REST / recording-service connectivity.
- No Eclipse/IDz preprocessor extension (wrong platform).
- No live CICS security enforcement; security metadata is carried, not enforced.
- No changes to `TestCase`/`Mutation` domain entities.

## Decisions

### 1. Keep `record` + `init` + `IReadOnlyList` (not RFC's mutable classes)

`ZUnitImportResult` stays a `record` and gains `Schema`, `RecordedEntries`, `GenConfig`, `RunnerResults`, `CicsContext` — all `init`, all defaulting to empty. The RFC's `public class` with `List<T> { get; set; }` is rejected: it diverges from the codebase's immutable-record convention.

**Alternatives considered:** adopt RFC class shape — rejected, breaks the existing `record : ImportResult` hierarchy and `init`-only construction.

### 2. Tolerant parsing for every new artifact

Each new parser follows the `ZUnitXmlParser` contract: `Parse(string) → *ParseResult { Payload, Warnings, IsValid }`, never throwing. New parsers under `Serialization/`: `XsdSchemaParser`, `RecordedDataXmlParser`, `GenerationConfigParser`, `RunnerResultsParser`, `CicsContextReader`.

### 3. XSD validation via built-in `System.Xml.Schema`

No new NuGet dependency. `XsdSchemaParser` loads an `.xsd` and optionally validates `.xml` test data, recording schema violations as warnings (never aborting import).

**Alternatives considered:** external validation libs — rejected, adds a dependency the PRD stack doesn't justify.

### 4. Recorded data maps IN→Inputs, OUT→ExpectedOutputs

`<compileunit type="IN">` feeds `TestCase.Inputs`; `type="OUT"` feeds `ExpectedOutputs`. The opaque `<data>` element is flattened tolerantly into key/value pairs (user decision: follow recommendation); unparseable content becomes a warning.

### 5. CICS context from a dedicated `cics-context.json`

A `cics-context.json` next to the other exports supplies `CICSContext` (region, tcpIpPort, pipeline, host, user ids, security, SIT params). The reader is tolerant; a missing/empty file is not an error.

**Alternatives considered:** read from CSD/SIT (mainframe) — rejected, CLI is offline; embed in `.bzucfg` — rejected, pollutes a separate concern.

### 6. Coverage map reuses already-computed data

`MutationEngine` already assigns `CoveringTestIds` per mutation (line-contains-input-key). `coverage-map.json` is a dedicated serialization of that mapping; no new mutation logic.

### 7. `.pb`, DTR, and preprocessor are inert stubs

`PlaybackDatasetReader` and any DTR/preprocessor surface realize their contracts but report "not yet supported", mirroring `TestAcceleratorPlugin`. Importing a `.pb` file records a warning.

### 8. Per-mutant test data (replicate)

`test-data.xml` is emitted per mutant, replicating the source program's `TestCase` inputs/outputs (user decision: replicate), so each `.cbl` mutant ships with its own test data for replay.

## Risks / Trade-offs

- **Undocumented ZUnit formats change across versions** → Mitigation: version-tolerant parsers; XSD validation catches drift; warnings instead of failures.
- **XSD validation on large files** → Mitigation: validate only when a schema is present; cap streaming via `XmlReader` where practical.
- **`<data>` flattening lossiness** → Mitigation: raw string retained alongside flattened map; unparseable content warns.
- **Manifest growth** → Mitigation: new fields are optional and empty by default; `schemaVersion` added for forward-compatibility.
- **Stubs may be mistaken for support** → Mitigation: stubs return explicit "not yet supported" messages and import warnings.
- **TAZ divergence** → Mitigation: new structures are framework-neutral; TAZ gets its own RFC/change and reuses them.
