## Why

The tool is a CLI, but there is no command surface yet. The PRD (section 5.2) specifies `init`, `generate`, `export`, `plugin list`, `--help`, and `--version`. This change wires the application and infrastructure layers together behind a System.CommandLine interface so users can drive the whole pipeline.

## What Changes

- Implement the CLI entry point (`Program.cs`) with System.CommandLine.
- Implement commands: `InitCommand`, `GenerateCommand`, `ExportCommand`, `PluginCommand` (list).
- Wire `--help` and `--version` (reporting the tool version).
- Implement `ServiceCollectionExtensions` for dependency injection of parsers, plugins, engine, and use cases.
- Support `--quiet` across commands.

## Capabilities

### New Capabilities
- `cli`: The command-line interface exposing init, generate, export, plugin, help, and version.

## Impact

- Populates `src/CobolMutantForge.CLI/` (Program, Commands, Extensions).
- Depends on all prior capabilities (domain, parser, plugins, engine, export).
- This is the user-facing surface; the tool becomes runnable end-to-end.
