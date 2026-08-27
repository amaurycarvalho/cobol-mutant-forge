## 1. Import structures

- [ ] 1.1 Add `TestDataSchema`, `RecordedData`, `CompileUnit`, `GenerationConfig`, `TestRunnerResults`, `CICSContext`, and `SecurityProfile` records under `Plugins/`
- [ ] 1.2 Extend `ZUnitImportResult` (record) with `Schema`, `RecordedEntries`, `GenConfig`, `RunnerResults`, and `CicsContext` (all `init`, default empty)
- [ ] 1.3 Add import tests for the new result fields

## 2. Import parsers

- [ ] 2.1 Implement `XsdSchemaParser` (`Serialization/`) loading an `.xsd` and validating `.xml` via `System.Xml.Schema`, recording violations as warnings
- [ ] 2.2 Implement `RecordedDataXmlParser` (`Serialization/`) parsing `batchrun`/`compileunit` and mapping `IN`→inputs / `OUT`→expected outputs
- [ ] 2.3 Implement `GenerationConfigParser` (`Serialization/`) for AZUGEN `.xml`
- [ ] 2.4 Implement `RunnerResultsParser` (`Serialization/`) for runner results `.xml`
- [ ] 2.5 Implement `CicsContextReader` (`Serialization/`) for `cics-context.json`
- [ ] 2.6 Add serialization tests for each new parser

## 3. Plugin wiring

- [ ] 3.1 Extend `ZUnitPlugin.Import` to scan `.xsd`, AZUGEN `.xml`, runner results `.xml`, and `cics-context.json`
- [ ] 3.2 Apply XSD validation to imported `.xml` test data when a schema is present
- [ ] 3.3 Add a `.pb` playback stub that records a "not yet supported" warning
- [ ] 3.4 Add plugin tests (`ZUnitPluginTests.cs`) for the new import paths

## 4. Export structures

- [ ] 4.1 Extend `ManifestDto` with optional `CicsContext`, `RecordedEntries`, and `SchemaVersion` fields
- [ ] 4.2 Add a `CoverageMapDto` for mutation-to-test-entry mapping
- [ ] 4.3 Update export DTO tests (`ExportDtosTests.cs`)

## 5. Export generation

- [ ] 5.1 Emit `test-data.xml` per mutant, replicating the source test case's inputs/outputs
- [ ] 5.2 Generate `test-data.xsd` describing the mutant test data layout
- [ ] 5.3 Emit `coverage-map.json` from the mutations' `CoveringTestIds`
- [ ] 5.4 Populate `cicsContext`, `recordedEntries`, and `schemaVersion` in the manifest
- [ ] 5.5 Add exporter tests (`MutantPackageExporterTests.cs`) for the new artifacts

## 6. Validation

- [ ] 6.1 Run `make build`, `make test`, and `make lint` and resolve failures
