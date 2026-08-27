# IBM ZUnit Plugin Expansion – RFC

**Document Number:** RFC-2026-08-27-001
**Status:** Draft
**Author:** CobolMutantForge Team
**Date:** 2026-08-27


## 1. Executive Summary

This RFC proposes an expansion of the IBM ZUnit Plugin scope within the CobolMutantForge ecosystem. The expansion enhances bidirectional integration with IBM ZUnit (z/OS Automated Unit Testing Framework), adding support for additional artifact types, improved mutation testing workflows, and deeper CICS transaction testing capabilities.

IBM ZUnit is an adaptation of the xUnit framework for Enterprise COBOL and PL/I, providing automated solutions for recording, running, and verifying unit test cases. The framework supports testing CICS applications by recording parameters used in EXEC CICS calls.


## 2. Background & Motivation

### 2.1 Current State
The current ZUnit Plugin supports:
- Import of `.xml` (test data), `.bzucfg` (configuration), `.cbl` (source), `.zip` (CICS projects), and `.cpy`/`.cob` (COPYBOOKS)
- Basic test case validation against expected results
- Mutant code generation and packaging

### 2.2 Identified Gaps
1. **Limited test data granularity** – No support for imported runtime data from IBM z/OS Debugger
2. **No XSD schema awareness** – Test data layout schemas (`.xsd`) are not consumed
3. **Missing recording/playback support** – No integration with Dynamic Test Runner (DTR) recording datasets
4. **No mutation coverage mapping** – Inability to correlate mutations with recorded test entries
5. **Limited CICS-specific metadata** – No capture of CICS pipeline, TCP/IP port, or security profile information

### 2.3 Deprecation Note
IBM has deprecated ZUnit in favor of IBM Test Accelerator for Z. This expansion ensures forward compatibility by designing import/export structures that can adapt to both frameworks.


## 3. Expanded Plugin Scope

### 3.1 New Objects Exported from CICS (Consumed)

| Type | Extension | Description | Usage in CobolMutantForge |
|:---|:---|:---|:---|
| **Test Data Schema** | `.xsd` | XML Schema Definition for test data layout | Validate test data structure before import |
| **Recorded Runtime Data** | `.xml` | Runtime data captured via z/OS Debugger (batchrun node structure) | Create test entries from actual execution traces |
| **Playback Dataset** | `.pb` | Dynamic Test Runner recording dataset | Replay recorded transactions for mutation validation |
| **Generation Config** | `.xml` | Test case generation configuration (AZUGEN) | Preserve generation options across mutation cycles |
| **Runner Results** | `.xml` | Test runner results with pass/fail statistics | Validate mutation outcomes against baseline |

### 3.2 Expanded Consumption Structure

```csharp
public class ZUnitImportResult
{
    public List<CobolProgram> Programs { get; set; }           // From .cbl
    public List<TestCase> TestCases { get; set; }              // From .xml
    public ZUnitConfig Config { get; set; }                    // From .bzucfg
    public List<Copybook> Copybooks { get; set; }              // From .cpy
    public TestDataSchema Schema { get; set; }                 // NEW: From .xsd
    public List<RecordedData> RecordedEntries { get; set; }    // NEW: From debugger XML
    public PlaybackMetadata Playback { get; set; }             // NEW: From .pb dataset
    public GenerationConfig GenConfig { get; set; }            // NEW: From AZUGEN .xml
    public TestRunnerResults RunnerResults { get; set; }       // NEW: From results .xml
    public CICSContext CicsContext { get; set; }               // NEW: CICS-specific metadata
    public List<string> Warnings { get; set; }
    public bool IsValid { get; set; }
}
```

### 3.3 CICS Context Structure

```csharp
public class CICSContext
{
    public string RegionName { get; set; }
    public string TcpIpPort { get; set; }                      // From CSD definition
    public string PipelineName { get; set; }                   // CICS pipeline for ZUnit communication
    public string HostName { get; set; }                       // System host for service listening
    public string ClientUserId { get; set; }                   // IDz client user ID
    public string CicsUserId { get; set; }                     // CICS region user ID
    public string CicsAdminId { get; set; }                    // CICS administration ID
    public string CicsRegionUserId { get; set; }               // CICS region execution user ID
    public SecurityProfile Security { get; set; }              // Security profiles and permissions
    public List<string> CicsSitParams { get; set; }            // SIT parameters (TCPIP=YES, USSHOME, etc.)
}
```

### 3.4 Recorded Data Structure (from Debugger XML)

Based on IBM documentation, recorded data XML follows this structure:

```xml
<batchrun>
  <compileunit>
    <extname>PROGRAM_NAME</extname>
    <type>IN|OUT</type>
    <index>1</index>
    <data><!-- recorded data values --></data>
  </compileunit>
</batchrun>
```

```csharp
public class RecordedData
{
    public string BatchRunId { get; set; }
    public List<CompileUnit> CompileUnits { get; set; }
}

public class CompileUnit
{
    public string ExtName { get; set; }                        // Program name
    public IoType Type { get; set; }                           // IN or OUT
    public int Index { get; set; }                             // Call count
    public object Data { get; set; }                           // Actual data values
}
```

### 3.5 Expanded Published Objects

| Type | Extension | Description |
|:---|:---|:---|
| **Mutant Code** | `.cbl` | COBOL program with mutations applied |
| **Manifest** | `manifest.json` | Metadata about applied mutations |
| **Report** | `mutations-report.json` | Detailed list of all mutations |
| **Package** | `.zip` | Bundle for import via CICS Explorer |
| **Test Data** | `.xml` | NEW: Generated test data for mutated program |
| **Data Schema** | `.xsd` | NEW: Schema for mutant test data |
| **Runner Config** | `.bzucfg` | NEW: Updated runner configuration for mutant |
| **Coverage Map** | `.json` | NEW: Mutation-to-test-entry coverage mapping |

### 3.6 Expanded Manifest Structure

```json
{
  "mutantId": "MUT-001-PAYMENT-INT-001",
  "originalProgram": "PAYMENT001",
  "baseProgramHash": "a1b2c3d4e5f6",
  "timestamp": "2026-08-27T14:30:00Z",
  "mutationProfile": "medium",
  "cicsContext": {
    "region": "CICSREG1",
    "tcpPort": 12345,
    "pipeline": "AZU_PIPELINE",
    "security": "SEC=YES"
  },
  "mutations": [
    {
      "id": "MUT-001",
      "type": "logical_operator",
      "line": 145,
      "original": "IF AMOUNT > 0 AND CUSTOMER-ACTIVE",
      "mutated": "IF AMOUNT > 0 OR CUSTOMER-ACTIVE",
      "testCaseCoverage": ["TC-001", "TC-003"]
    }
  ],
  "recordedEntries": [
    {
      "entryId": "REC-001",
      "compileUnit": "PAYMENT001",
      "ioType": "IN",
      "data": { "AMOUNT": 1500.00, "CUSTOMER-ACTIVE": true }
    }
  ],
  "sourceCopied": true,
  "copybooksResolved": true,
  "schemaVersion": "2.0"
}
```


## 4. Technical Design

### 4.1 Import Pipeline

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  CICS Artifacts │───▶│  ZUnit Parser    │───▶│  Schema         │
│  (.cbl, .xml,   │    │  (COBOL + XML)   │    │  Validator      │
│   .xsd, .bzucfg)│    │                  │    │  (XSD)          │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                           │
                                                           ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Test Entry     │◀───│  Data Mapper     │◀───│  Recorded Data  │
│  Repository     │    │  (Runtime →      │    │  Importer       │
│                 │    │   Test Case)     │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### 4.2 Mutation Workflow with DTR Integration

1. **Record**: User executes CICS transaction; DTR captures runtime data
2. **Import**: Recorded data is imported as ZUnit test entries
3. **Mutate**: CobolMutantForge applies syntactic mutations to COBOL source
4. **Generate**: Updated test data and runner configuration are generated
5. **Replay**: DTR replays recorded transactions against mutated program
6. **Validate**: Results are compared against expected outcomes

### 4.3 Data Set Allocation

Based on ZUnit property group requirements, the plugin must support:

| Data Set | Format | Purpose |
|:---|:---|:---|
| `<HLQ>.ZUNIT.AZUGEN` | VB/VBA | Generation configuration |
| `<HLQ>.ZUNIT.COBOL` | F/FB | Test case programs |
| `<HLQ>.ZUNIT.AZUSCH` | VB/VBA | Test data layout (XSD) |
| `<HLQ>.ZUNIT.AZUTDT` | VB/VBA, RECL=16383 | Test data XML |
| `<HLQ>.ZUNIT.PB` | — | Playback recording dataset |

### 4.4 ZUnit Preprocessor Extension

IBM ZUnit supports custom preprocessor extensions via the `com.ibm.etools.zunit.common.preProcessor` extension point. The expanded plugin will:

1. Register as a COBOL ZUnit Preprocessor
2. Intercept test case generation to inject mutation metadata
3. Preserve original generation options across mutation cycles

### 4.5 Runner Configuration (BZUCFG) Enhancements

The `.bzucfg` file defines compiler options, link options, and test runner configuration. Expanded support includes:

```csharp
public class BZUConfig
{
    public string Type { get; set; }                           // Subsystem type
    public List<TestCaseRef> TestCases { get; set; }          // Test cases to run
    public CompilerOptions Compiler { get; set; }
    public LinkOptions Link { get; set; }
    public string DestinationContainer { get; set; }          // MVS dataset or z/OS UNIX folder
    public bool DynamicTestRunner { get; set; }               // Use DTR vs legacy runner
}
```


## 5. Integration Points

### 5.1 With IBM z/OS Debugger

- Import linkage data XML files generated by the debugger
- Support batchrun/compileunit/extname/data node structure

### 5.2 With Dynamic Test Runner

- Consume playback datasets (`.pb`) for transaction replay
- Configure DTR recording service URL
- Support both recording and replay modes

### 5.3 With CICS Explorer

- Export mutation packages as `.zip` bundles
- Include CICS bundle structure reference
- Support import via CICS Explorer UI


## 6. Non-Functional Requirements

### 6.1 Performance
- XML validation must complete within 5 seconds for files up to 16MB
- Support batch processing of multiple test entries from single recording

### 6.2 Compatibility
- Support ZUnit versions 14.0.0 and later (new runner API)
- Maintain compatibility with older runner API where applicable
- Support COBOL and PL/I test cases

### 6.3 Security
- Respect CICS security profiles (SEC=YES, CMDSEC=ALWAYS, RESSEC=ALWAYS)
- Handle CICS region user ID permissions
- Support READ access to AZUMCICS transaction profiles


## 7. Implementation Roadmap

### Phase 1: Import Enhancement (Sprint 1-2)
- Add `.xsd` schema import and validation
- Implement recorded data XML parser (batchrun structure)
- Expand `ZUnitImportResult` with new structures

### Phase 2: DTR Integration (Sprint 3-4)
- Implement playback dataset (`.pb`) consumption
- Add recording/replay workflow support
- Integrate with DTR recording service

### Phase 3: Export Enhancement (Sprint 5-6)
- Generate test data XML for mutants
- Generate `.xsd` schemas for mutant test data
- Update `.bzucfg` generation with DTR support

### Phase 4: Preprocessor Extension (Sprint 7-8)
- Register COBOL ZUnit preprocessor extension
- Implement mutation metadata injection
- Preserve generation options across cycles

### Phase 5: CICS Context & Reporting (Sprint 9-10)
- Capture and propagate CICS context metadata
- Generate coverage maps linking mutations to test entries
- Produce comprehensive mutation reports


## 8. Risks and Mitigations

| Risk | Impact | Mitigation |
|:---|:---|:---|
| ZUnit deprecation | Medium | Design for IBM Test Accelerator for Z compatibility |
| XML schema changes across versions | Medium | Version-aware parsers; schema validation before import |
| DTR API changes | Low | Abstract DTR interaction layer |
| CICS security configuration complexity | Medium | Provide validation and clear error messages |
| Large XML files (>16MB) | Low | Implement streaming parser for large files |


## 9. References

1. IBM Developer for z/OS Documentation – ZUnit (z/OS Automated Unit Testing Framework)
2. IBM Developer for z/OS – XUnit Support for CICS Applications (ZUnit)
3. IBM Developer for z/OS – Importing I/O Data Recorded in Batch
4. IBM Developer for z/OS – Setting Options for Creating/Modifying Test Case Files
5. IBM Z Virtual Test Platform – Dynamic Test Runner
6. IBM Developer for z/OS – ZUnit Preprocessor Plugin Extensions
7. IBM Developer for z/OS – Unit Testing Enterprise COBOL and PL/I Applications
