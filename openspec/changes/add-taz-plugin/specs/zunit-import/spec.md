## MODIFIED Requirements

### Requirement: Test Accelerator stub

The system SHALL provide a `TestAcceleratorPlugin` implementing `PluginBase`, `IImportPlugin`, and `IExportPlugin`. It SHALL perform real import/export of TAZ artifacts (see `taz-plugin`), while its runtime surfaces (CLI executor, REST client, code coverage collector) remain inert stubs that report "not yet supported".

#### Scenario: Import and export are functional

- **WHEN** the Test Accelerator plugin is invoked to import or export TAZ artifacts
- **THEN** it performs the operation rather than reporting "not yet supported"

#### Scenario: Runtime surfaces remain inert

- **WHEN** a runtime surface (CLI executor, REST client, or code coverage collector) is invoked
- **THEN** it reports that it is not yet supported
