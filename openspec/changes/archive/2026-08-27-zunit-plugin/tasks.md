## 1. Plugin abstraction

- [x] 1.1 Implement `Plugins/PluginBase.cs`
- [x] 1.2 Implement `Plugins/ZUnitPlugin.cs` realizing `IImportPlugin`
- [x] 1.3 Implement `Plugins/TestAcceleratorPlugin.cs` stub (inert, marked v2.0)

## 2. ZUnit result and config

- [x] 2.1 Implement `ZUnitImportResult` (programs, test cases, config, copybooks, warnings, validity)
- [x] 2.2 Implement `ZUnitConfig` type

## 3. Serialization

- [x] 3.1 Implement `Serialization/ZUnitXmlParser.cs` for `.xml` test data → `TestCase`
- [x] 3.2 Implement `.bzucfg` parsing into `ZUnitConfig`

## 4. Source and copybook loading

- [x] 4.1 Load `.cbl` files into `CobolProgram` instances
- [x] 4.2 Resolve COPYBOOK references from the configured copybook directory
- [x] 4.3 Warn on missing copybooks and malformed artifacts without hard-failing

## 5. Verification

- [x] 5.1 Import a sample ZUnit export directory and assert the result is valid
- [x] 5.2 Import a directory with a missing copybook and assert a warning is recorded
