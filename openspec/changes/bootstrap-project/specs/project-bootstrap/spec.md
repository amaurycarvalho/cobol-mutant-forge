## ADDED Requirements

### Requirement: Solution and project layout

The project SHALL provide a single root solution `CobolMutantForge.sln` containing five projects under the `src/` and `tests/` directories: `CobolMutantForge.Domain`, `CobolMutantForge.Application`, `CobolMutantForge.Infrastructure`, `CobolMutantForge.CLI`, and `CobolMutantForge.Tests`.

#### Scenario: Solution builds cleanly

- **WHEN** `dotnet build CobolMutantForge.sln` is executed from the repository root
- **THEN** all five projects compile without errors

#### Scenario: Dependency direction is respected

- **WHEN** the project-to-project references are inspected
- **THEN** `Application` references `Domain`, `Infrastructure` references `Application`, `CLI` references `Application` and `Infrastructure`, and `Tests` references all source projects, with no reference pointing from a core layer to an outer layer

### Requirement: Target framework

All projects SHALL target .NET 8.0 LTS.

#### Scenario: Framework verification

- **WHEN** any `.csproj` file is inspected
- **THEN** it declares `<TargetFramework>net8.0</TargetFramework>`

### Requirement: Configuration file schema

The tool SHALL define a `cobolmutantforge.json` configuration contract with the following top-level sections: `projectName`, `version`, `paths` (`sourceDirectory`, `testDataDirectory`, `outputDirectory`, `copybookDirectory`), `mutationProfile` (`low` | `medium` | `high`), `mutationFlags` (`logicalOperators`, `arithmeticOperators`, `complexExpressions`, `numericConstants`, `stringConstants`), `zunit`, `testAccelerator`, and `export`.

#### Scenario: Default configuration is serializable

- **WHEN** a default configuration object is serialized with System.Text.Json
- **THEN** the output JSON contains every top-level section defined above

#### Scenario: Mutation profile values are constrained

- **WHEN** the `mutationProfile` field is deserialized
- **THEN** only the values `low`, `medium`, and `high` are accepted

### Requirement: Logging foundation

The CLI SHALL be wired with a logging abstraction (Microsoft.Extensions.Logging with a console provider) that supports an informational output mode and a `--quiet` error-only mode.

#### Scenario: Quiet mode suppresses informational messages

- **WHEN** the CLI is invoked with the quiet flag enabled
- **THEN** informational messages are suppressed while error messages remain visible
