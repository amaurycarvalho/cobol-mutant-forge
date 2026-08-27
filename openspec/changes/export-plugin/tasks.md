## 1. Manifest and report

- [ ] 1.1 Implement `manifest.json` serialization matching the PRD structure
- [ ] 1.2 Implement `mutations-report.json` generation enumerating all mutations
- [ ] 1.3 Derive `baseProgramHash` and mutation id scheme

## 2. Exporter

- [ ] 2.1 Implement `Exporters/MutantPackageExporter.cs` realizing `IExportPlugin`
- [ ] 2.2 Implement `zip` output mode
- [ ] 2.3 Implement `folder` output mode
- [ ] 2.4 Record `sourceCopied` and `copybooksResolved` flags

## 3. Application use case

- [ ] 3.1 Implement `ExportMutantsUseCase`

## 4. Verification

- [ ] 4.1 Export a sample mutant set as `zip` and assert the archive contains `.cbl` files, `manifest.json`, and `mutations-report.json`
- [ ] 4.2 Export as `folder` and assert the same contents on disk
- [ ] 4.3 Assert the manifest contains all PRD-defined fields
