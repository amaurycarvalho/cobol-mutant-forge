# IBM Test Accelerator for Z – Technical Documentation & Plugin RFC Extension

## 1. Executive Summary

IBM Test Accelerator for Z (TAZ) is a comprehensive test automation and generation framework purpose-built for z/OS developers and testers. It enables teams to adopt agile and continuous integration practices for z/OS applications through three core capabilities:

| Capability                          | Description                                                                                                                                               |
| :---------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Early Development Testing (EDT)** | Creation and execution of reusable unit and component tests that can run without middleware; available as VS Code and Eclipse extensions                  |
| **Dynamic Test Engine (DTE)**       | Enables test creation from recordings and provides capability to run unit tests without middleware; automatically captures data accessed by EDT extension |
| **On-Demand Environments (ODE)**    | Provision z/OS instances for development and testing via Web UI or REST APIs; instances can be sourced from stock or custom images                        |

**Version 2.0** (announced August 2025) delivers enhanced unit and functional testing capabilities, including RBAC interface and improved instance controller.

## 2. Architecture Overview

### 2.1 Component Architecture

TAZ follows a distributed architecture with the following key components:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Client Tier                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │
│  │ VS Code      │  │ Eclipse IDz  │  │ Web UI (ODE)         │ │
│  │ EDT Extension│  │ EDT Plugin   │  │ (Image/Instance Mgmt)│ │
│  └──────────────┘  └──────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    API / CLI Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │
│  │ REST APIs    │  │ taz CLI      │  │ Debug Profile Service│ │
│  │ (ODE)        │  │ (Unit Test)  │  │ API / IMS ISO API    │ │
│  └──────────────┘  └──────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Dynamic Test Engine (z/OS)                  │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ z/OS Debugger (v17) + Data Collector + EQAPROF services │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    z/OS Target Environments                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │
│  │ CICS TS      │  │ IMS TM       │  │ Batch / Db2 for z/OS │ │
│  └──────────────┘  └──────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Deployment Artifacts (v2.0)

| Component                         | Part No. | File                                        |
| :-------------------------------- | :------- | :------------------------------------------ |
| TAZ 2.0 eAssembly                 | G0G4ZML  | —                                           |
| Quick Start Guide                 | M0V0GML  | PDF                                         |
| ODE Installer (Linux x86/s390x)   | M0V0HML  | ode-install-2.0.tgz                         |
| EDT on Eclipse (Windows P2)       | M0V0JML  | edt-eclipse-win-p2-2.0.0.zip                |
| EDT on VS Code                    | M0V0KML  | taz-edt-extension-1.0.0.vsix                |
| Functional Testing on VS Code     | M0V0LML  | ft-vscode-2.0.zip                           |
| IBM Enterprise Edition for Galasa | M0V0MML  | IBM-Enterprise-Edition-for-Galasa-1.1.4.zip |
| DTE Host Component (SMPE)         | M0V0NML  | TAZ_DTE_Host_SMPE-2.0.zip                   |

## 3. Technical Interfaces

### 3.1 REST APIs

TAZ provides REST APIs for On-Demand Environments operations:

- **Base Path**: `/api/v1`
- **Debug Profile Service API**: Manages EQAUOPTS datasets for debug sessions
- **IMS Transaction Isolation Service API**: Manages IMS isolation operations
- **Image Management**: Create, duplicate, version, and delete z/OS images
- **Instance Provisioning**: Provision z/OS instances to Linux targets
- **CORS Support**: Configurable via `corAllowedOrigins` parameter

Key configuration file: `/etc/debug/eqaprof.env` contains server, API, and security parameters:

```bash
java_dir="/usr/lpp/java/J8.0_64"
liberty_dir="/usr/lpp/liberty_zos"
port="8143"
SECURE="Y"
context_path="/api/v1"
requestsPerSec=1000.0
```

### 3.2 Command-Line Interface (taz CLI)

The `taz` CLI provides unit test execution capabilities:

**Syntax**:

```
taz unittest run <testDirectory> [options]
```

or

```
taz ut run <testDirectory> [options]
```

**Required Parameters**:
| Parameter | Description |
|:---|:---|
| `--procLib` | PROCLIB dataset (e.g., `CUST.V70.PROCLIB`) |
| `--userLibrary` | User load library containing programs under test; can be specified multiple times |

**Optional Parameters**:
| Parameter | Description |
|:---|:---|
| `--rootDirectory` | Base directory for resolving relative paths |
| `--timeout` | Max wait time for job completion in seconds (default: 120) |
| `--jobCard` | Path to JCL job card file |
| `--json` | Output results in JSON format |
| `--output` | Results folder location (default: `.taz-edt-results`) |
| `-cc` / `--codeCoverage` | Run with code coverage (`<ip:port>`) |
| `-cp` / `--codeCoverageSourcePath` | z/OS Unix path for source lookup |
| `-cs` / `--codeCoverageCertificate` | Certificate for authenticated code coverage collector |
| `--keep-files-job-cc-ne-0` | Generate diagnostic file |
| `--keep-all-files` | Generate diagnostic file |

**Global Options**: `-v|--verbose`, `-q|--quiet`, `--no-color`, `-h|--help`, `-V|--version`

## 4. File Formats & Data Structures

### 4.1 Test Case Files (`.ztest`)

TAZ uses proprietary `.ztest` files for test case definitions:

- **Location**: Stored on z/OS Unix file systems
- **Creation**: Must be created and modified using the EDT VS Code extension; CLI does not support creation
- **Validation**: Semantic validation for `programIndex`, `occurrence`, and `dataID` to ensure logical consistency and detect invalid references
- **Error Handling**: Errors in `.ztest` files are displayed in the Problems tab within VS Code
- **Discovery**: CLI recursively searches for `*.ztest` files in specified directories

### 4.2 Test Data Files (`.zdata`)

- **Format**: Only JSON format test data files (`.zdata`) are supported
- **Binary Format**: Binary format test data files are **not** supported

### 4.3 Test Results

Test results are saved in XML format that can be opened in the Unit Test Runner Results viewer. Export capabilities include:

- Export runner configurations for running tests that generated results
- Export runner results into **JUnit** or **PDF** format

### 4.4 Project Configuration (`zapp.yaml` / `zapp.json`)

A `zapp.yaml` or `zapp.json` file is stored at the root of each project and can be shared using modern SCM.

### 4.5 Test Data Layout Files (`AZUSCH`)

When new syntax is supported in the Test Case Editor, the format of test data layout files (`AZUSCH`) may change.

### 4.6 Legacy Test Case Files (`.tc`)

Existing test case files (`.tc`) can be opened with the Test Case Editor.

## 5. Plugin RFC: Extended Scope

### 5.1 Revised Objects to be Consumed

| Type                   | Format                    | Source                  | Consumption Method           |
| :--------------------- | :------------------------ | :---------------------- | :--------------------------- |
| **Test Configuration** | `zapp.yaml` / `zapp.json` | Project root            | File parsing                 |
| **Test Cases**         | `.ztest`                  | z/OS Unix directories   | File system access or API    |
| **Test Data**          | `.zdata` (JSON)           | z/OS Unix directories   | File system access or API    |
| **Test Results**       | XML / JUnit / JSON        | `.taz-edt-results/`     | File parsing or CLI `--json` |
| **Code Coverage**      | Via `-cc` flag            | Code coverage collector | API integration              |
| **Images**             | ODE stock/custom images   | ODE REST API            | REST API calls               |
| **Instances**          | z/OS instances            | ODE REST API            | REST API calls               |

### 5.2 Expanded Plugin Architecture

```csharp
public class TestAcceleratorPlugin : PluginBase, IImportPlugin, IExportPlugin,
                                     IExecutionPlugin, IValidationPlugin
{
    // Core components
    private ITazCliExecutor _cliExecutor;
    private ITazRestApiClient _apiClient;
    private ITestCaseParser _testCaseParser;
    private ITestDataParser _testDataParser;
    private IResultParser _resultParser;
    private ICodeCoverageCollector _coverageCollector;

    // Configuration
    private TazPluginConfiguration _configuration;
}

public interface ITazCliExecutor
{
    /// <summary>
    /// Executes taz unit test command and returns results
    /// </summary>
    Task<TazTestResult> RunUnitTestsAsync(
        string testDirectory,
        string procLib,
        string[] userLibraries,
        TazCliOptions options,
        CancellationToken cancellationToken = default
    );
}

public interface ITazRestApiClient
{
    // ODE Image Management
    Task<Image> CreateImageAsync(CreateImageRequest request);
    Task<Image> GetImageAsync(string imageId);
    Task<Image> DuplicateImageAsync(string imageId, string newName);
    Task<Image> CreateImageVersionAsync(string imageId, Component[] components);
    Task DeleteImageAsync(string imageId);

    // Instance Management
    Task<Instance> ProvisionInstanceAsync(string imageId, TargetEnvironment target);
    Task<Instance> GetInstanceAsync(string instanceId);
    Task StopInstanceAsync(string instanceId);
    Task StartInstanceAsync(string instanceId);
    Task DeleteInstanceAsync(string instanceId);

    // Debug Profile Service
    Task<DebugProfile> CreateDebugProfileAsync(DebugProfileRequest request);
}

public interface ITestCaseParser
{
    /// <summary>
    /// Parses .ztest file format - proprietary format to be reverse-engineered
    /// </summary>
    Task<TestCase> ParseZTestAsync(string filePath);
    Task<TestCase> ParseZTestAsync(Stream stream);

    /// <summary>
    /// Validates .ztest file for semantic consistency
    /// </summary>
    Task<ValidationResult> ValidateZTestAsync(string filePath);
}

public interface ITestDataParser
{
    /// <summary>
    /// Parses .zdata JSON test data files
    /// </summary>
    Task<TestData> ParseZDataAsync(string filePath);
    Task<TestData> ParseZDataAsync(Stream stream);
}

public interface IResultParser
{
    /// <summary>
    /// Parses test results from XML, JUnit, or JSON formats
    /// </summary>
    Task<TestExecutionResult> ParseResultsAsync(string resultPath, ResultFormat format);
}

public interface ICodeCoverageCollector
{
    /// <summary>
    /// Collects code coverage data from headless collector
    /// </summary>
    Task<CoverageReport> CollectCoverageAsync(string host, int port, string sourcePath);
}

public class TazPluginConfiguration
{
    public string TazInstallPath { get; set; }
    public string DefaultProcLib { get; set; }
    public string[] DefaultUserLibraries { get; set; }
    public string OdeApiBaseUrl { get; set; }
    public string OdeApiKey { get; set; }
    public int DefaultTimeoutSeconds { get; set; } = 120;
    public bool EnableCodeCoverage { get; set; }
    public string CodeCoverageCollectorHost { get; set; } = "localhost";
    public int CodeCoverageCollectorPort { get; set; } = 8143;
}
```

### 5.3 Identified Technical Specifications

| Aspect                   | Specification                                            | Source |
| :----------------------- | :------------------------------------------------------- | :----- |
| **CLI Tool**             | `taz` command with `unittest run` / `ut run` subcommands |        |
| **Test File Extension**  | `.ztest` (test cases)                                    |        |
| **Data File Extension**  | `.zdata` (JSON only)                                     |        |
| **Results Format**       | XML (native), JUnit, PDF (export), JSON (`--json` flag)  |        |
| **Results Location**     | `.taz-edt-results/` (default)                            |        |
| **Project Config**       | `zapp.yaml` / `zapp.json`                                |        |
| **API Base Path**        | `/api/v1`                                                |        |
| **API Port**             | 8143 (default)                                           |        |
| **IDE Support**          | VS Code (extension) + Eclipse (IDz plugin)               |        |
| **Supported Languages**  | IBM Enterprise COBOL 4.1+ (PL/I planned)                 |        |
| **Supported Middleware** | CICS TS, IMS TM, Batch, Db2 for z/OS                     |        |
| **Code Coverage**        | Via `-cc <ip:port>` flag, requires headless collector    |        |

### 5.4 Implementation Strategy

#### Phase 1: CLI Integration (Foundation)

1. Implement `ITazCliExecutor` wrapping the `taz` CLI
2. Parse CLI output (plain text and JSON modes)
3. Support all documented CLI parameters
4. Handle timeout, verbose, and diagnostic file generation

#### Phase 2: File Format Parser

1. **`.ztest` parser**: Reverse-engineer proprietary format
   - Leverage VS Code extension (open-source? investigate)
   - Analyze sample `.ztest` files
   - Implement semantic validation (programIndex, occurrence, dataID)
2. **`.zdata` parser**: JSON format (straightforward)
3. **`zapp.yaml` / `zapp.json` parser**: Standard YAML/JSON
4. **Result parser**: XML, JUnit, JSON support

#### Phase 3: REST API Integration

1. Implement `ITazRestApiClient` for ODE operations
2. Support authentication (API keys, certificates)
3. Implement all CRUD operations for images and instances
4. Integrate Debug Profile Service API

#### Phase 4: Code Coverage Integration

1. Connect to headless code coverage collector
2. Support authenticated collector (certificate-based)
3. Parse coverage reports
4. Map coverage to source files

#### Phase 5: VS Code Extension Compatibility (Optional)

1. Investigate VS Code extension internals (`taz-edt-extension-*.vsix`)
2. Determine if extension provides extensibility points
3. Consider direct integration vs. CLI-based approach

### 5.5 Known Challenges & Mitigations

| Challenge                                       | Mitigation                                                                                               |
| :---------------------------------------------- | :------------------------------------------------------------------------------------------------------- |
| **No documented public API for test artifacts** | Use CLI as primary integration point; reverse-engineer file formats                                      |
| **Proprietary `.ztest` format**                 | Analyze with sample files; potentially leverage existing open-source parsers or VS Code extension source |
| **Native integration with IBM IDEs**            | Support CLI-based workflow as universal integration point                                                |
| **z/OS file system access**                     | Use z/OS Unix file system access (SSH, FTP, or z/OSMF)                                                   |
| **Authentication complexity**                   | Support multiple auth methods: API keys, certificates (PKCS12, JCERACFKS, JCECCARACFKS)                  |
| **Code coverage collector setup**               | Document prerequisites; support `-cc`, `-cp`, `-cs` flags                                                |
| **VS Code extension versioning**                | Monitor extension updates; maintain compatibility matrix                                                 |

### 5.6 Plugin Configuration Schema

```yaml
# plugin-config.yaml
testAccelerator:
  installPath: /opt/ibm/taz
  cli:
    defaultTimeout: 120
    defaultProcLib: CUST.V70.PROCLIB
    userLibraries:
      - CUST.V70.LOADLIB
      - CUST.V70.TESTLIB

  onDemandEnvironments:
    apiBaseUrl: https://taz-ode.example.com:8143/api/v1
    auth:
      type: certificate # apiKey | certificate | none
      keystorePath: /path/to/keystore.p12
      keystoreType: PKCS12
      keystorePassword: ${TAZ_KEYSTORE_PASSWORD}

  codeCoverage:
    enabled: true
    collectorHost: localhost
    collectorPort: 8143
    certificatePath: /path/to/cert.pem

  resultProcessing:
    outputDirectory: .taz-edt-results
    formats:
      - xml
      - json
      - junit
    exportEnabled: true
```

### 5.7 Extended Plugin Interface

```csharp
public class TestAcceleratorPlugin : PluginBase,
                                     IImportPlugin,
                                     IExportPlugin,
                                     IExecutionPlugin,
                                     IValidationPlugin,
                                     ICoveragePlugin
{
    public override string Id => "ibm.testaccelerator";
    public override string Name => "IBM Test Accelerator for Z";
    public override string Version => "2.0.0";

    // IImportPlugin
    public async Task<ImportResult> ImportAsync(ImportRequest request);

    // IExportPlugin
    public async Task<ExportResult> ExportAsync(ExportRequest request);

    // IExecutionPlugin
    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request);

    // IValidationPlugin
    public async Task<ValidationResult> ValidateAsync(ValidationRequest request);

    // ICoveragePlugin
    public async Task<CoverageResult> GetCoverageAsync(CoverageRequest request);
}
```

### 5.8 References & Resources

| Resource                       | URL                                                                                                            |
| :----------------------------- | :------------------------------------------------------------------------------------------------------------- |
| IBM Documentation (TAZ 2.x)    | https://www.ibm.com/docs/en/test-accelerator-for-z/2.x                                                         |
| TAZ Announcement               | https://www.ibm.com/docs/en/announcements/test-accelerator-z-v2-helps-accelerate-mainframe-application-testing |
| Software Product Compatibility | https://www.ibm.com/software/reports/compatibility/clarity/index.html                                          |
| Host Configuration Assistant   | https://zdev-hca.ibm.com                                                                                       |
| VS Code Marketplace            | https://marketplace.visualstudio.com/items?itemName=IBM.taz-edt                                                |
| Passport Advantage             | https://www.ibm.com/software/passportadvantage/                                                                |
| IBM Fix Central                | https://www.ibm.com/support/fixcentral                                                                         |

---

_This RFC is based on publicly available IBM documentation for Test Accelerator for Z v2.x and is intended as a technical planning document for plugin development. All proprietary formats and APIs should be validated against actual product implementations._
