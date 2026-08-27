## Why

The ZUnit plugin only consumes a narrow slice of ZUnit's exports (`.xml`, `.bzucfg`, `.cbl`, COPYBOOKS) and packages mutants without the artifacts needed to validate them against recorded runtime data. This means the tool cannot correlate mutations to real test entries, validate test data against its schema, or carry CICS context — the exact gaps that keep mutation testing shallow for CICS programs (per RFC-002).

## What Changes

- **Import** (`zunit-import`): the plugin gains tolerant parsers for `.xsd` test-data schemas (with optional XSD validation of `.xml`), recorded runtime data (`batchrun`/`compileunit`), AZUGEN generation configs, and runner results; plus a `cics-context.json` reader. `ZUnitImportResult` is extended with the new structures.
- **Export** (`mutant-packaging`): the exporter emits `test-data.xml` per mutant (replicating the source test case's inputs/outputs), a generated `test-data.xsd`, and a `coverage-map.json` (mutation-to-test-entry mapping). `manifest.json` gains optional `cicsContext`, `recordedEntries`, and `schemaVersion` fields.
- **Playback/DTR/preprocessor**: realized as inert stubs (pattern of `TestAcceleratorPlugin`) reporting "not yet supported" rather than parsing undocumented binary/network formats.

## Capabilities

### New Capabilities

### Modified Capabilities

- `zunit-import`: the import SHALL consume `.xsd` schemas (and validate test data), recorded runtime data, generation configs, runner results, and CICS context.
- `mutant-packaging`: the exporter SHALL emit per-mutant test data XML, a test-data schema, a coverage map, and an expanded manifest.

## Impact

- **Infrastructure**: `ZUnitPlugin`, `ZUnitImportResult`, `ZUnitConfig`, new serialization parsers (`Serialization/`), `MutantPackageExporter`, `ExportDtos`.
- **Domain**: `TestCase` is reused for recorded-data mapping; no entity changes required.
- **No new external dependencies**: XSD validation uses built-in `System.Xml.Schema`.
- **No breaking changes**: new manifest fields and import structures default to empty, so existing configs/packages remain valid.
