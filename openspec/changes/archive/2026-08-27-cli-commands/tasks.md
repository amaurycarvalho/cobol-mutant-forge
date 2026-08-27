## 1. Entry point and DI

- [x] 1.1 Implement `Program.cs` with System.CommandLine root command
- [x] 1.2 Implement `Extensions/ServiceCollectionExtensions.cs` registering parsers, plugins, engine, use cases, and exporters
- [x] 1.3 Wire console logging with quiet-mode filtering

## 2. Commands

- [x] 2.1 Implement `Commands/InitCommand.cs`
- [x] 2.2 Implement `Commands/GenerateCommand.cs`
- [x] 2.3 Implement `Commands/ExportCommand.cs`
- [x] 2.4 Implement `Commands/PluginCommand.cs` (list)

## 3. Help and version

- [x] 3.1 Wire `--help` for all commands
- [x] 3.2 Wire `--version` reporting the tool version
- [x] 3.3 Add `--quiet` to all commands

## 4. Verification

- [x] 4.1 Run `cobol-mutant-forge --version` and confirm the version prints
- [x] 4.2 Run `cobol-mutant-forge init` and confirm `cobolmutantforge.json` is created
- [x] 4.3 Run `cobol-mutant-forge plugin list` and confirm both plugins are listed
- [x] 4.4 Run `cobol-mutant-forge export --format folder` on a sample mutant set
