## 1. Strengthen existing tests to kill "Survived" mutants (76)

- [x] 1.1 Strengthen `TypeCobolParserAdapterTests` string-mutation assertions to check exact node/diagnostic text and locations (~30 kills)
- [x] 1.2 Strengthen `CliIntegrationTests` to assert concrete command output/content for `GenerateCommand`, `ExportCommand`, `InitCommand`, `PluginCommand` (~34 kills)
- [x] 1.3 Strengthen `DomainModelTests` profile-matrix assertions to cover every flag of Low/Medium/High and `FromName` error paths (~9 kills)
- [x] 1.4 Strengthen `ServiceCollectionExtensions` and `ExportJsonOptions` coverage in existing CLI/Exporter tests (~3 kills)

## 2. Domain + Application coverage (99 no-coverage mutants)

- [x] 2.1 Expand `DomainModelTests` for `Mutation`, `CobolProgram`, `TestCase`, `MutantPackage` equality/hash/null-guard branches (~33 kills)
- [x] 2.2 Add tests for `MutationProject` aggregate (add/remove programs and test cases, null guards) (~7 kills)
- [x] 2.3 Add tests for `AstNode` and `ParseResult`/`ImportResult` interfaces (~5 kills)
- [x] 2.4 Add `ValidationServiceTests` covering all validation branches (~12 kills)
- [x] 2.5 Expand `GenerateMutationsUseCaseTests` and `ExportMutantsUseCaseTests` for error and edge paths (~16 kills)
- [x] 2.6 Add `Configuration` tests for `DefaultConfigFactory`, `PathsDto`, `MutationConfigDto` (~20 kills)

## 3. Infrastructure data layers (Serialization/Parsers/Plugins/Exporters/Configuration)

- [x] 3.1 Expand `ZUnitConfigParserTests` for all formats, malformed input, and warning paths (~61 kills)
- [x] 3.2 Expand `ZUnitXmlParserTests` for all XML shapes and error handling (~44 kills)
- [x] 3.3 Expand `TypeCobolParserAdapterTests` for IF/COMPUTE/loop/string/comment/error constructs (~107 kills)
- [x] 3.4 Expand `ZUnitPluginTests` and `TestAcceleratorPluginTests` for full import/export cycles (~73 kills)
- [x] 3.5 Expand `MutantPackageExporterTests` and `PackageManifestReaderTests` for all export/manifest paths (~67 kills)
- [x] 3.6 Add `JsonConfigSerializerTests` coverage for remaining serialization branches (~7 kills)

## 4. Mutation engine and strategies (130 no-coverage mutants)

- [x] 4.1 Expand `MutationEngineTests` to cover both constructors, null guards, and the `IsEnabled` switch for every `OperationType` (~60 kills)
- [x] 4.2 Expand `MutationStrategyTests` for `LogicalOperatorMutationStrategy` and `ArithmeticOperatorMutationStrategy` including `Ast is null` and all node kinds (~59 kills)
- [x] 4.3 Add coverage for `ConstantMutationStrategy`, `ComplexExpressionMutationStrategy`, and `AstTraversal` (~11 kills)

## 5. Verification

- [x] 5.1 Run `make test` and confirm all tests pass and are xUnit1051-compliant
- [x] 5.2 Run `make mutation` and confirm the final mutation score is greater than 80%
