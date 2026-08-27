## 1. Solution and project scaffolding

- [x] 1.1 Create `CobolMutantForge.sln` at the repository root
- [x] 1.2 Create `src/CobolMutantForge.Domain/CobolMutantForge.Domain.csproj` targeting `net8.0`
- [x] 1.3 Create `src/CobolMutantForge.Application/CobolMutantForge.Application.csproj` targeting `net8.0` with a reference to Domain
- [x] 1.4 Create `src/CobolMutantForge.Infrastructure/CobolMutantForge.Infrastructure.csproj` targeting `net8.0` with a reference to Application
- [x] 1.5 Create `src/CobolMutantForge.CLI/CobolMutantForge.CLI.csproj` targeting `net8.0` with references to Application and Infrastructure
- [x] 1.6 Create `tests/CobolMutantForge.Tests/CobolMutantForge.Tests.csproj` targeting `net8.0` with references to all source projects
- [x] 1.7 Add all five projects to the solution

## 2. Package wiring

- [x] 2.1 Add `System.CommandLine` package to the CLI project
- [x] 2.2 Add `Microsoft.Extensions.Logging` and a console logging provider to the CLI project
- [x] 2.3 Add `xunit` `3.2.2` and `xunit.runner.visualstudio` to the Tests project
- [x] 2.4 Add `System.Text.Json` usage to the Application and Infrastructure projects

## 3. Configuration contract

- [x] 3.1 Define the configuration DTO model (`MutationConfigDto`) in Application with all PRD sections
- [x] 3.2 Implement default configuration creation with `mutationProfile = medium`
- [x] 3.3 Implement `JsonConfigSerializer` in Infrastructure (serialize + deserialize)
- [x] 3.4 Constrain `mutationProfile` to `low`, `medium`, `high`

## 4. Base logging

- [x] 4.1 Wire a console logger in the CLI entry point
- [x] 4.2 Implement quiet-mode filtering that suppresses informational messages

## 5. Verification

- [x] 5.1 Confirm `dotnet build CobolMutantForge.sln` succeeds
- [x] 5.2 Confirm a serialized default config round-trips through deserialization
