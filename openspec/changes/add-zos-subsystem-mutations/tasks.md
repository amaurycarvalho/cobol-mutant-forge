## 1. Domain model

- [ ] 1.1 Extend `MutationType` enum (`ValueObjects/MutationType.cs`) with CICS, IMS DL/I, ADABAS, and VSAM mutation types per design.md §Decisions #3
- [ ] 1.2 Extend `OperationType` enum (`ValueObjects/OperationType.cs`) with `Cics`, `Ims`, `Adabas`, `Vsam`
- [ ] 1.3 Add `Cics`, `Ims`, `Adabas`, `Vsam` boolean flags to `MutationProfile` (`ValueObjects/MutationProfile.cs`), with `false` in `Low`/`Medium` and `true` in `High`
- [ ] 1.4 Update domain model tests (`DomainModelTests.cs`) for the new enum members and profile flags

## 2. Parser extension

- [ ] 2.1 Add new subsystem keywords to `KnownStatementKeywords` in `TypeCobolParserAdapter.cs` (`EXEC`, `CICS`, `END-EXEC`, `SELECT`, `ASSIGN`, `ORGANIZATION`, `ACCESS`, `RECORD`, `KEY`, `START`, `SYNCPOINT`, `XCTL`, `RETURN`, `LINK`, `VSAM-CODE`)
- [ ] 2.2 Recognize single-line `EXEC CICS ... END-EXEC` and emit `CicsStatement` + `CicsOption` nodes
- [ ] 2.3 Recognize `CALL "CBLTDLI"/"CEETDLI" USING ...` and emit `ImsDliStatement` nodes
- [ ] 2.4 Recognize `CALL 'ADABAS' USING ...` and emit `AdabasStatement` nodes
- [ ] 2.5 Recognize `SELECT ... ASSIGN` file control entries and `READ`/`WRITE`/`REWRITE`/`DELETE`/`START` operations, emitting `VsamFileControlEntry` and `VsamFileOperation` nodes
- [ ] 2.6 Add parser tests (`TypeCobolParserAdapterTests.cs`) for each subsystem node kind

## 3. Subsystem mutation strategies

- [ ] 3.1 Implement `CicsCommandMutationStrategy` (`Mutators/`) with the command-verb mapping and single-line `SYNCPOINT` removal
- [ ] 3.2 Implement `ImsDliMutationStrategy` (`Mutators/`) with the function-code mapping
- [ ] 3.3 Implement `AdabasMutationStrategy` (`Mutators/`) for optional buffer argument removal
- [ ] 3.4 Implement `VsamMutationStrategy` (`Mutators/`) for `ACCESS MODE` and `WRITE`↔`REWRITE` mutations
- [ ] 3.5 Add strategy tests (`MutationStrategyTests.cs`) for each subsystem strategy

## 4. Engine wiring

- [ ] 4.1 Register the four new strategies in the `MutationEngine` default constructor
- [ ] 4.2 Extend `MutationEngine.IsEnabled` to map the new `OperationType` values to profile flags
- [ ] 4.3 Add engine-level tests (`MutationEngineTests.cs`) verifying subsystem mutations are generated under `high` and suppressed under `low`/`medium`

## 5. Configuration

- [ ] 5.1 Add `Cics`, `Ims`, `Adabas`, `Vsam` flags to `MutationFlagsDto` (`Application/Configuration/`)
- [ ] 5.2 Map the new flags in `DefaultConfigFactory` so they default off and are serialized
- [ ] 5.3 Update configuration tests (`ConfigurationTests.cs`, `JsonConfigSerializerTests.cs`) for the new fields

## 6. BDD and validation

- [ ] 6.1 Add BDD scenarios to `MutationGeneration.feature` for CICS and IMS DL/I mutations
- [ ] 6.2 Wire the new scenarios through `MutationGenerationSteps.cs`
- [ ] 6.3 Run `make build`, `make test`, and `make lint` and resolve failures
