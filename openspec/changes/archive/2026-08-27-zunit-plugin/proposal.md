## Why

CobolMutantForge's input is the set of objects exported by IBM ZUnit from CICS (`.xml` test data, `.bzucfg` configuration, `.cbl` source, and COPYBOOKS). Without an importer that understands these artifacts, the tool cannot assemble the `MutationProject` that drives generation. This change delivers the ZUnit plugin.

## What Changes

- Implement `ZUnitPlugin` implementing `IImportPlugin` (and ready to implement `IExportPlugin` for output).
- Parse `.xml` test data into `TestCase` objects and `.bzucfg` into `ZUnitConfig`.
- Load `.cbl` source into `CobolProgram` instances and resolve COPYBOOK dependencies.
- Produce a `ZUnitImportResult` with programs, test cases, config, copybooks, warnings, and validity.
- Add a `PluginBase` abstraction and the `TestAcceleratorPlugin` stub (v2.0 placeholder).

## Capabilities

### New Capabilities
- `zunit-import`: Importing ZUnit-exported objects (XML, config, COBOL source, copybooks) into the domain model.

## Impact

- Populates `src/CobolMutantForge.Infrastructure/Plugins/` and `Serialization/`.
- Adds the `ZUnitXmlParser` and `ZUnitConfig` types from the PRD.
- Depends on `domain-model` and `type-cobol-parser`.
