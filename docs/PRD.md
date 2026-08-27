# Product Requirements Document (PRD)

## **CobolMutantForge** 🧬

**Version:** 1.0  
**Date:** August 27, 2026  
**Status:** Draft  
**Language:** English

---

## 1. Executive Summary

### 1.1 Product Name
**CobolMutantForge** - A Mutation Testing Tool for COBOL/CICS Programs

### 1.2 Vision Statement
To empower COBOL/CICS development teams with automated mutation testing capabilities, bridging the gap between legacy mainframe testing practices and modern software quality assurance methodologies.

### 1.3 Problem Statement
- COBOL/CICS developers lack modern mutation testing tools
- Manual mutation testing is time-consuming and error-prone
- Existing test suites (ZUnit) are underutilized for quality assessment
- Limited visibility into test suite effectiveness

### 1.4 Solution Overview
CobolMutantForge is a CLI tool written in C#/.NET Core that:
- Consumes existing IBM ZUnit test artifacts (XML, configuration, source code)
- Applies systematic code mutations to COBOL programs
- Generates mutant packages for manual import into CICS
- Provides configurable mutation profiles (low/medium/high)
- Extensible architecture for future integration with IBM Test Accelerator for Z

---

## 2. Scope

### 2.1 In-Scope (MVP v1.0)
- ✅ CLI interface with basic commands (`init`, `generate`, `export`, `help`)
- ✅ Integration with IBM ZUnit via exported objects (`.xml`, `.bzucfg`, `.cbl`, COPYBOOKS)
- ✅ Mutations on logical operators (`AND`↔`OR`, `NOT` insertion/removal)
- ✅ Mutations on arithmetic operators (`+`↔`-`, `*`↔`/`)
- ✅ JSON configuration file with mutation profiles
- ✅ Package generation (ZIP) for manual import into CICS Explorer
- ✅ Extensible plugin architecture for import/export
- ✅ Unit tests (xUnit v3.2.2)
- ✅ BDD scenarios (MTP v1)
- ✅ Stryker.NET integration for mutation testing the tool itself

### 2.2 Out-of-Scope (v1.0)
- ❌ Full automation of compilation/link-edit on mainframe
- ❌ Direct deployment to CICS regions
- ❌ IBM Test Accelerator for Z integration (planned for v2.0)
- ❌ Mutations on complex expressions and constants (planned for v2.0)
- ❌ GUI interface
- ❌ Real-time test execution validation

---

## 3. Technology Stack

| Component | Technology | Version/Specification |
|:---|:---|:---|
| **Language** | C# | .NET Core 8.0 LTS |
| **Architecture** | Clean Architecture + DDD | - |
| **COBOL Parser** | TypeCobol | Latest stable (GitHub) |
| **Unit Testing** | xUnit | v3.2.2 |
| **BDD** | MTP (Meet The Pragma) | v1 |
| **Mutation Testing (Tool)** | Stryker.NET | Configured via `stryker-config.json` |
| **CLI Framework** | System.CommandLine | - |
| **Serialization** | System.Text.Json | - |
| **Logging** | Serilog or Microsoft.Extensions.Logging | - |
| **Build Automation** | GitHub Actions or Azure DevOps | - |

---

## 4. Architecture

### 4.1 Clean Architecture + DDD Structure

```
CobolMutantForge.sln
├── src/
│   ├── CobolMutantForge.Domain/               # Entities, Value Objects, Aggregates
│   │   ├── Entities/
│   │   │   ├── CobolProgram.cs
│   │   │   ├── Mutation.cs
│   │   │   ├── TestCase.cs
│   │   │   └── MutantPackage.cs
│   │   ├── ValueObjects/
│   │   │   ├── OperationType.cs
│   │   │   ├── MutationType.cs
│   │   │   └── MutationProfile.cs
│   │   ├── Aggregates/
│   │   │   └── MutationProject.cs
│   │   └── Interfaces/
│   │       ├── ICobolParser.cs
│   │       ├── IMutationStrategy.cs
│   │       ├── IImportPlugin.cs
│   │       └── IExportPlugin.cs
│   │
│   ├── CobolMutantForge.Application/          # Use Cases, DTOs, Services
│   │   ├── UseCases/
│   │   │   ├── GenerateMutationsUseCase.cs
│   │   │   ├── ExportMutantsUseCase.cs
│   │   │   └── ImportTestsUseCase.cs
│   │   ├── DTOs/
│   │   │   ├── MutationConfigDto.cs
│   │   │   └── MutantResultDto.cs
│   │   └── Services/
│   │       ├── MutationEngine.cs
│   │       └── ValidationService.cs
│   │
│   ├── CobolMutantForge.Infrastructure/       # Concrete Implementations
│   │   ├── Parsers/
│   │   │   └── TypeCobolParserAdapter.cs
│   │   ├── Plugins/
│   │   │   ├── ZUnitPlugin.cs                 # IBM ZUnit Plugin
│   │   │   ├── TestAcceleratorPlugin.cs       # Test Accelerator Plugin (stub)
│   │   │   └── PluginBase.cs
│   │   ├── Serialization/
│   │   │   ├── ZUnitXmlParser.cs
│   │   │   └── JsonConfigSerializer.cs
│   │   └── Exporters/
│   │       └── MutantPackageExporter.cs
│   │
│   ├── CobolMutantForge.CLI/                  # Command Line Interface
│   │   ├── Program.cs
│   │   ├── Commands/
│   │   │   ├── InitCommand.cs
│   │   │   ├── GenerateCommand.cs
│   │   │   ├── ExportCommand.cs
│   │   │   └── PluginCommand.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   └── CobolMutantForge.Tests/                # Tests
│       ├── Unit/                              # xUnit v3.2.2
│       │   ├── Domain/
│       │   └── Application/
│       ├── BDD/                               # MTP v1
│       │   ├── Features/
│       │   │   └── MutationGeneration.feature
│       │   └── Steps/
│       └── stryker-config.json                # Stryker.NET Configuration
│
└── README.md
```

### 4.2 High-Level Flow Diagram

```
[ZUnit Export] → [ZUnit Plugin] → [CobolMutantForge Core]
      ↓                                         ↓
   Objects:                                AST COBOL +
   .xml (test data)                       Configuration
   .bzucfg (config)                            ↓
   .cbl (source)                       [Mutation Engine]
   COPYBOOKS                                   ↓
      ↓                                    [Mutants]
      ↓                                        ↓
[Manual Import] ← [Export Plugin] ← [ZIP Package]
 CICS Explorer                                   ↓
                                            [.cbl files]
                                         + manifest.json
```

---

## 5. Detailed Features

### 5.1 Project Configuration Module

#### 5.1.1 Configuration File (`cobolmutantforge.json`)

```json
{
  "projectName": "CobolMutantForge_Project",
  "version": "1.0.0",
  "paths": {
    "sourceDirectory": "./src",
    "testDataDirectory": "./zunit-exports",
    "outputDirectory": "./mutants",
    "copybookDirectory": "./copybooks"
  },
  "mutationProfile": "medium", // low | medium | high
  "mutationFlags": {
    "logicalOperators": true,
    "arithmeticOperators": true,
    "complexExpressions": false,
    "numericConstants": false,
    "stringConstants": false
  },
  "zunit": {
    "enabled": true,
    "expectedXmlPattern": "*.xml",
    "configPattern": "*.bzucfg",
    "sourcePattern": "*.cbl"
  },
  "testAccelerator": {
    "enabled": false,
    "plannedFor": "v2.0"
  },
  "export": {
    "format": "zip",
    "includeManifest": true,
    "manifestFormat": "json"
  }
}
```

#### 5.1.2 Mutation Profiles

| Profile | Logical Ops | Arithmetic Ops | Complex Expressions | Numeric Constants | String Constants |
|:---|:---:|:---:|:---:|:---:|:---:|
| **Low** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Medium** | ✅ | ✅ | ❌ | ✅ | ❌ |
| **High** | ✅ | ✅ | ✅ | ✅ | ✅ |

### 5.2 CLI Commands

#### 5.2.1 `cobol-mutant-forge init`

**Description**: Automatically creates a `cobolmutantforge.json` configuration file based on a directory structure.

```bash
cobol-mutant-forge init [directory] [--profile low|medium|high] [--quiet]
```

- If `directory` is not specified, uses current directory
- If `directory` is empty or no objects are found, creates file with default parameters (profile `medium`)
- `--quiet`: Suppresses informational messages (errors only)

#### 5.2.2 `cobol-mutant-forge generate`

**Description**: Generates mutants based on project configuration.

```bash
cobol-mutant-forge generate [--config cobolmutantforge.json] [--plugin zunit] [--output ./mutants]
```

- `--config`: Path to configuration file (default: `./cobolmutantforge.json`)
- `--plugin`: Plugin to use (`zunit` or `testaccelerator`)
- `--output`: Output directory (overrides config)
- `--quiet`: Suppresses informational messages

#### 5.2.3 `cobol-mutant-forge export`

**Description**: Generates package for manual import into CICS.

```bash
cobol-mutant-forge export --source ./mutants --output ./packages --format zip
```

- `--source`: Directory containing generated mutants
- `--output`: Output directory for the package
- `--format`: Package format (`zip` or `folder`)

#### 5.2.4 `cobol-mutant-forge plugin list`

**Description**: Lists all available plugins.

```bash
cobol-mutant-forge plugin list
```

#### 5.2.5 `cobol-mutant-forge --help`

**Description**: Displays detailed help for all commands.

#### 5.2.6 `cobol-mutant-forge --version`

**Description**: Displays the tool version.

### 5.3 IBM ZUnit Plugin

#### 5.3.1 Objects Exported from CICS (Consumed)

| Type | Extension | Description | Usage in CobolMutantForge |
|:---|:---|:---|:---|
| **Test Data** | `.xml` | CICS transaction execution records (inputs/outputs) | Validate mutant behavior against expected results |
| **Configuration** | `.bzucfg` | ZUnit Test Runner configuration | Identify test parameters and context |
| **Source Code** | `.cbl` | Original COBOL program | Apply syntactical mutations |
| **CICS Project** | `.zip` | CICS Bundle (optional) | Reference structure for packages |
| **COPYBOOKS** | `.cpy` or `.cob` | COBOL include files | Resolve code dependencies |

#### 5.3.2 Consumption Structure

```csharp
public class ZUnitImportResult
{
    public List<CobolProgram> Programs { get; set; }        // From .cbl
    public List<TestCase> TestCases { get; set; }           // From .xml
    public ZUnitConfig Config { get; set; }                 // From .bzucfg
    public List<Copybook> Copybooks { get; set; }           // From .cpy
    public List<string> Warnings { get; set; }              // Logs
    public bool IsValid { get; set; }
}
```

#### 5.3.3 Published Objects (For CICS Import)

| Type | Extension | Description |
|:---|:---|:---|
| **Mutant Code** | `.cbl` | COBOL program with mutations applied |
| **Manifest** | `manifest.json` | Metadata about applied mutations |
| **Report** | `mutations-report.json` | Detailed list of all mutations |
| **Package** | `.zip` | Bundle for import via CICS Explorer |

#### 5.3.4 Manifest Structure

```json
{
  "mutantId": "MUT-001-PAYMENT-INT-001",
  "originalProgram": "PAYMENT001",
  "baseProgramHash": "a1b2c3d4e5f6",
  "timestamp": "2026-08-27T14:30:00Z",
  "mutationProfile": "medium",
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
  "sourceCopied": true,
  "copybooksResolved": true
}
```

### 5.4 IBM Test Accelerator for Z Plugin (Stub for v2.0)

**Status**: Planned for v2.0 (preliminary scope defined)

#### 5.4.1 Objects to be Consumed (Preliminary)

| Type | Description | Notes |
|:---|:---|:---|
| **Test Configuration** | Test Accelerator configuration files | Proprietary format to be studied |
| **Test Cases** | Test data sets | Potentially exportable via API |
| **Test Results** | Execution reports | For mutant validation |

#### 5.4.2 Identified Challenges

- No documented public API
- Proprietary file formats
- Native integration with IBM IDEs

#### 5.4.3 Preliminary Plugin Structure

```csharp
public class TestAcceleratorPlugin : PluginBase, IImportPlugin, IExportPlugin
{
    // Under development for v2.0
    // Priority: Understand Test Accelerator export formats
}
```

---

## 6. Mutation Engine

### 6.1 Supported Operators (Simplified Scope)

#### 6.1.1 Logical Operators

| Original | Mutation | Description |
|:---|:---|:---|
| `AND` | `OR` | Replace conjunction with disjunction |
| `OR` | `AND` | Replace disjunction with conjunction |
| `NOT (condition)` | `(condition)` | Remove negation |
| `(condition)` | `NOT (condition)` | Add negation |

#### 6.1.2 Arithmetic Operators

| Original | Mutation | Description |
|:---|:---|:---|
| `+` | `-` | Replace addition with subtraction |
| `-` | `+` | Replace subtraction with addition |
| `*` | `/` | Replace multiplication with division |
| `/` | `*` | Replace division with multiplication |
| Complex expression | `0` or `1` | Replace with constant |

### 6.2 Mutation Engine Interface

```csharp
public interface IMutationEngine
{
    List<Mutation> GenerateMutations(CobolProgram program, MutationProfile profile);
    CobolProgram ApplyMutation(CobolProgram program, Mutation mutation);
    bool ValidateMutation(CobolProgram program, Mutation mutation);
}
```

### 6.3 Mutation Strategies (Strategy Pattern)

```csharp
public interface IMutationStrategy
{
    bool CanApply(AstNode node);
    List<Mutation> Apply(AstNode node);
    MutationType Type { get; }
}
```

Implementations:
- `LogicalOperatorMutationStrategy`
- `ArithmeticOperatorMutationStrategy`
- `ConstantMutationStrategy`
- `ComplexExpressionMutationStrategy`

---

## 7. Testing & Quality Assurance

### 7.1 Unit Tests (xUnit v3.2.2)

```csharp
[Fact]
public void GenerateMutations_LogicalOperator_ShouldReturnTwoMutations()
{
    // Arrange
    var program = new CobolProgram("IF A > B AND C = D");
    var engine = new MutationEngine();
    
    // Act
    var mutations = engine.GenerateMutations(program, MutationProfile.Medium);
    
    // Assert
    Assert.Equal(2, mutations.Count);
    Assert.Contains(mutations, m => m.MutatedCode.Contains("OR"));
}
```

### 7.2 BDD Tests (MTP v1)

**Feature**: Mutation Generation

```gherkin
Feature: Mutation Generation
  As a COBOL developer
  I want to automatically generate mutants
  So that I can validate the quality of my ZUnit tests

  Scenario: Generate mutant with logical operator
    Given a COBOL program with the condition "IF AMOUNT > 0 AND CUSTOMER-ACTIVE"
    And a mutation profile "medium"
    When I execute the "generate" command
    Then a mutant with "OR" replacing "AND" should be generated
    And the mutant should be saved as a .cbl file

  Scenario: Generate mutant with arithmetic operator
    Given a COBOL program with the expression "COMPUTE TOTAL = AMOUNT + TAX"
    And a mutation profile "medium"
    When I execute the "generate" command
    Then a mutant with "-" replacing "+" should be generated
```

### 7.3 xUnit1051 Warning Handling

**Warning**: `xUnit1051` - "Calls to assertion methods should not be used within loops"

**Solution**:
```csharp
// ❌ Avoid
[Theory]
[InlineData(1)]
[InlineData(2)]
public void BadTest(int value)
{
    foreach(var item in collection)
    {
        Assert.Equal(value, item); // xUnit1051
    }
}

// ✅ Correct
[Theory]
[InlineData(1)]
[InlineData(2)]
public void GoodTest(int value)
{
    Assert.All(collection, item => Assert.Equal(value, item));
}
```

### 7.4 Stryker.NET Configuration

**File**: `stryker-config.json`

```json
{
  "test-runner": "dotnet",
  "project-file": "CobolMutantForge.Tests.csproj",
  "mutate": [
    "src/CobolMutantForge.Domain/**/*.cs",
    "src/CobolMutantForge.Application/**/*.cs",
    "!**/*.Tests.cs"
  ],
  "reporters": ["html", "progress"],
  "thresholds": {
    "high": 80,
    "low": 60
  },
  "ignore-methods": [
    "ToString",
    "GetHashCode"
  ],
  "test-projects": ["CobolMutantForge.Tests"]
}
```

---

## 8. Additional Features (Suggested for Future)

### 8.1 Mutant Validation
- Verify mutant compiles syntactically
- Validate structural integrity (same number of sections, paragraphs)

### 8.2 Reporting
- Generate HTML report with all applied mutations
- Heat map showing most mutated areas

### 8.3 Parsing Cache
- Store parsed AST for reuse
- Avoid reparsing in subsequent executions

### 8.4 Multiple Profile Support
- `low`: Only 1 mutation per operator (fast)
- `medium`: 2-3 mutations per expression (balanced)
- `high`: All possible combinations (exhaustive)

### 8.5 Interactive Mode
- Allow manual selection of mutations to apply
- Before/after code visualization

### 8.6 Git Integration
- Create separate branch for each mutant
- Auto-commit generated mutants

---

## 9. Deliverables

### 9.1 MVP (v1.0)
- ✅ CLI with basic commands (`init`, `generate`, `export`, `help`, `plugin`)
- ✅ Complete ZUnit Plugin (consumes `.xml`, `.bzucfg`, `.cbl`, COPYBOOKS)
- ✅ Logical and arithmetic operator mutations
- ✅ ZIP package generation for manual import
- ✅ JSON configuration file with profiles
- ✅ Unit tests (xUnit v3.2.2)
- ✅ BDD tests (MTP v1)
- ✅ Stryker.NET integration
- ✅ Basic documentation

### 9.2 Future Version (v2.0)
- 🔄 IBM Test Accelerator for Z Plugin
- 🔄 Complex expression and constant mutations
- 🔄 Interactive mode
- 🔄 HTML reports
- 🔄 Git integration

---

## 10. Estimated Timeline (MVP)

| Phase | Activity | Estimate |
|:---|:---|:---:|
| **1** | Project setup, architecture design | 1 week |
| **2** | TypeCobol parser integration | 2 weeks |
| **3** | ZUnit Plugin (consumption) | 2 weeks |
| **4** | Mutation Engine (operators) | 2 weeks |
| **5** | Export Plugin (ZIP packages) | 1 week |
| **6** | CLI and commands | 1 week |
| **7** | Testing (xUnit + MTP) | 1 week |
| **8** | Documentation and final adjustments | 1 week |
| **Total** | | **11 weeks (~3 months)** |

---

## 11. Risks and Mitigations

| Risk | Probability | Impact | Mitigation |
|:---|:---:|:---:|:---|
| TypeCobol doesn't support all COBOL constructs | Medium | High | Validate with real programs; consider ANTLR fallback |
| ZUnit XML format is undocumented | High | Medium | Reverse engineer; export multiple examples |
| Performance on large programs | Medium | Medium | Implement parsing cache; parallel processing |
| COBOL parser complexity | High | High | Start with simple operators; evolve incrementally |
| IBM ZUnit changes | Low | Medium | Version the format; warn on breaking changes |

---

## 12. Acceptance Criteria (MVP)

- [ ] Functional CLI with all basic commands
- [ ] Successful consumption of ZUnit exported objects
- [ ] Generation of at least 3 mutation types (AND↔OR, +↔-, *↔/)
- [ ] Auto-generated configuration file via `init`
- [ ] ZIP packages generated with CICS Explorer-compatible structure
- [ ] Unit tests with > 80% coverage
- [ ] Documented and passing BDD scenarios
- [ ] Stryker.NET configuration integrated
- [ ] Usage documentation (README + examples)
- [ ] Automated build (GitHub Actions or Azure DevOps)

---

## 13. Success Metrics

| Metric | Target |
|:---|:---:|
| **Time to generate mutants** | < 5 minutes for 1000 LOC |
| **Mutation coverage** | > 60% of target operators |
| **Test suite effectiveness** | Increase by 20% in mutant detection |
| **User adoption (6 months)** | 10+ teams |
| **Build stability** | > 95% pass rate |
| **Documentation completeness** | 100% of features documented |

---

## 14. Open Questions

- [ ] Should we support additional COBOL dialects (Enterprise COBOL, Micro Focus)?
- [ ] What is the maximum program size we should support?
- [ ] Should we include a mutant execution validation feature?
- [ ] How to handle COPYBOOK conflicts between versions?
- [ ] Should we generate both mutated source AND compiled modules?

---

## 15. Glossary

| Term | Definition |
|:---|:---|
| **AST** | Abstract Syntax Tree - tree representation of source code structure |
| **BDD** | Behavior-Driven Development - testing methodology using business language scenarios |
| **CICS** | Customer Information Control System - IBM transaction server for mainframes |
| **COPYBOOK** | COBOL file containing reusable code or data definitions |
| **DDD** | Domain-Driven Design - software design approach focusing on business domain |
| **MTP** | Meet The Pragma - a BDD framework for .NET |
| **Mutant** | A program version with one syntactical change applied |
| **Mutation Testing** | Testing technique where code is modified to check test effectiveness |
| **ZUnit** | IBM's unit testing framework for COBOL/zOS programs |

---

## 16. References

- IBM ZUnit Documentation
- TypeCobol GitHub Repository
- Stryker.NET Documentation
- xUnit v3 Documentation
- COBOL Language Reference

---

> CobolMutantForge project is freeware. 
> Feel free to contribute.
