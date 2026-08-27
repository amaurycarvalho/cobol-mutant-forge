## Why

The repository currently contains only project metadata (README, Makefile, CI workflows, OpenSpec scaffolding) inherited from an unrelated template. There is no .NET solution or source tree for CobolMutantForge, so the tool cannot be built or shipped until the base solution, project layout, and configuration schema are established.

## What Changes

- Create the `CobolMutantForge.sln` root solution targeting .NET 8.0 LTS.
- Scaffold the five projects defined by the architecture: Domain, Application, Infrastructure, CLI, and Tests.
- Establish the Clean Architecture + DDD folder structure (`src/` + `tests/`).
- Introduce the base configuration file schema (`cobolmutantforge.json`) and its serialization contract.
- Wire foundational dependencies (System.CommandLine, System.Text.Json, xUnit v3.2.2, TypeCobol, logging) at the project level.
- Add solution-wide build configuration and a `.slnx`-agnostic directory layout under `src/` and `tests/`.

## Capabilities

### New Capabilities
- `project-bootstrap`: The .NET solution, project layout, and base configuration schema that every other capability builds upon.

## Impact

- Creates `CobolMutantForge.sln` and the `src/` / `tests/` directory tree.
- Adds five `.csproj` files with correct project-to-project references.
- Defines the `cobolmutantforge.json` contract consumed by the CLI and application layers.
- No existing code is modified.
