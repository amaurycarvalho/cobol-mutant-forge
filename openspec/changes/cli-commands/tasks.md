## 1. Entry point and DI

- [ ] 1.1 Implement `Program.cs` with System.CommandLine root command
- [ ] 1.2 Implement `Extensions/ServiceCollectionExtensions.cs` registering parsers, plugins, engine, use cases, and exporters
- [ ] 1.3 Wire console logging with quiet-mode filtering

## 2. Commands

- [ ] 2.1 Implement `Commands/InitCommand.cs`
- [ ] 2.2 Implement `Commands/GenerateCommand.cs`
- [ ] 2.3 Implement `Commands/ExportCommand.cs`
- [ ] 2.4 Implement `Commands/PluginCommand.cs` (list)

## 3. Help and version

- [ ] 3.1 Wire `--help` for all commands
- [ ] 3.2 Wire `--version` reporting the tool version
- [ ] 3.3 Add `--quiet` to all commands

## 4. Verification

- [ ] 4.1 Run `cobol-mutant-forge --version` and confirm the version prints
- [ ] 4.2 Run `cobol-mutant-forge init` and confirm `cobolmutantforge.json` is created
- [ ] 4.3 Run `cobol-mutant-forge plugin list` and confirm both plugins are listed
- [ ] 4.4 Run `cobol-mutant-forge export --format folder` on a sample mutant set
