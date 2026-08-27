## 1. Typed configuration

- [ ] 1.1 Add `TazPluginConfiguration` record (`Application/Configuration/`) with `InstallPath`, `ProcLib`, `UserLibraries`, `OdeApiBaseUrl`, `TimeoutSeconds`, and code coverage fields (all empty/default)
- [ ] 1.2 Replace `MutationConfigDto.TestAccelerator` dictionary with the typed `TazPluginConfiguration`
- [ ] 1.3 Emit the typed default in `DefaultConfigFactory`
- [ ] 1.4 Update configuration tests (`ConfigurationTests.cs`, `JsonConfigSerializerTests.cs`) for the typed field

## 2. Import parsers

- [ ] 2.1 Implement `TazProjectConfigParser` (`Infrastructure/Serialization/`) for `zapp.json` with a `zapp.yaml` "not yet supported" warning
- [ ] 2.2 Implement `ZDataParser` (`Infrastructure/Serialization/`) for `.zdata` JSON test data
- [ ] 2.3 Implement `TazResultParser` (`Infrastructure/Serialization/`) for JUnit XML and JSON results
- [ ] 2.4 Add serialization tests for each new parser

## 3. Runtime ports (stubbed)

- [ ] 3.1 Add `ITazCliExecutor`, `ITazRestApiClient`, and `ICodeCoverageCollector` ports under `Domain/Interfaces/`
- [ ] 3.2 Add "not yet supported" implementations for each port in `Infrastructure/Plugins/`
- [ ] 3.3 Add tests verifying the stubs report "not yet supported"

## 4. TestAcceleratorPlugin

- [ ] 4.1 Implement `TestAcceleratorPlugin.Import` to scan `zapp.json`, `.zdata`, and results, warning on `.ztest`/`zapp.yaml`
- [ ] 4.2 Implement `TestAcceleratorPlugin.Export` delegating to the framework-neutral manifest/coverage-map packaging
- [ ] 4.3 Register the new parser dependencies in `ServiceCollectionExtensions`
- [ ] 4.4 Add plugin tests (`TestAcceleratorPluginTests.cs`) for import and export paths

## 5. CLI status

- [ ] 5.1 Update `PluginCommand` so `testaccelerator` reports `available` once functional
- [ ] 5.2 Update CLI integration tests for the new availability status

## 6. Validation

- [ ] 6.1 Run `make build`, `make test`, and `make lint` and resolve failures
