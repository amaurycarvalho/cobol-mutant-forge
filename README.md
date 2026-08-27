# CobolMutantForge

A Mutation Testing Tool for COBOL/CICS (z/OS) Programs.

[![Spec-Driven Development](https://img.shields.io/badge/SDD-OpenSpec-yellow)](openspec/specs/project-constitution/spec.md)

---

## For Users

### Features

CobolMutantForge is an open-source, freeware command-line tool for mutation testing of COBOL/CICS (z/OS) programs. It runs from the terminal and integrates with any development environment, giving developers full control over their mutation testing projects.

> This is an MVP experimental prototype in a very early stage.
> The project is currently "in development".
> Feel free to test and contribute.

### Requirements

- .NET SDK 8.0 (only needed to build from source or run via `dotnet run`)
- Linux, macOS, or Windows

### How to Install

Pre-built, self-contained executables are published with every release. No .NET SDK is required to run them.

1. Go to the [Releases](https://github.com/amaurycarvalho/cobol-mutant-forge/releases) page.
2. Download the archive for your platform:
   - `cobol-mutant-forge-<version>-win-x64.zip` for Windows (x64)
   - `cobol-mutant-forge-<version>-linux-x64.tar.gz` for Linux (x64)
   - `cobol-mutant-forge-<version>-osx-x64.tar.gz` for macOS (Intel)
   - `cobol-mutant-forge-<version>-osx-arm64.tar.gz` for macOS (Apple Silicon)
3. Extract the archive and run the `cobol-mutant-forge` executable.

### How to Use

Show allowed parameters:

```
cobol-mutant-forge --help
```

Run mutation testing against a COBOL source tree with a configuration file:

```
cobol-mutant-forge --config cobolmutantforge.json
```

Use `--quiet` to suppress informational output and show only errors.

#### Command-line parameters

```
Using:
  CobolMutantForge.CLI [command] [options]

Options:
  --quiet         Suppress informational output; only errors are shown.
  -?, -h, --help  Show help and usage information
  --version       Mostrar informações de versão

Commands:
  init      Create a cobolmutantforge.json configuration file.
  generate  Generate mutants based on the project configuration.
  export    Package generated mutants for manual CICS import.
  plugin    Inspect available plugins.
```

- `--quiet`: suppresses informational messages, printing only errors.
- `-?, -h, --help`: shows help and usage information for every command.
- `--version`: prints the tool version.
- `init [--directory <dir>] [--profile <low|medium|high>]`: creates a `cobolmutantforge.json` in the given directory (default: current directory, `medium` profile).
- `generate [--config <file>] [--plugin <zunit|testaccelerator>] [--output <dir>]`: generates mutants from the project configuration into the output directory.
- `export --source <dir> --output <dir> [--format <zip|folder>]`: packages generated mutants into a ZIP (or folder) for manual CICS import.
- `plugin list`: lists all available plugins with their availability status.

Every command also accepts `--quiet`.

---

## For Developers

### Specifications

This project uses [Spec-Driven Development (SDD)](https://opencode.ai). All specifications live under [`openspec/specs/`](openspec/specs/):

- Active changes are tracked under [`openspec/changes/`](openspec/changes/).
- Archived changes are tracked under [`openspec/changes/archive/`](openspec/changes/archive/).

### How to Get the Source Code

```bash
git clone https://github.com/amaurycarvalho/cobol-mutant-forge.git
```

### How to Install and Build

Requirements:

- .NET SDK 8.0
- Make

```bash
make install
make build
```

### How to Test the Application After a Build

After `make build`, run the CLI directly from the build output:

```bash
./src/CobolMutantForge.CLI/bin/Release/net8.0/CobolMutantForge.CLI --help
```

The `--help` option lists all available commands and options. You can also invoke the tool with a configuration file:

```bash
./src/CobolMutantForge.CLI/bin/Release/net8.0/CobolMutantForge.CLI --config cobolmutantforge.json
```

Or via the .NET CLI, which rebuilds and runs the project:

```bash
dotnet run --project src/CobolMutantForge.CLI -- --help
```

Use `--quiet` to suppress informational messages and show only errors.

### Linting and Unit Testing

```bash
make lint test
```

### Quality Gate

The quality gate runs linting and the unit tests:

```bash
make quality-gate
```

### Mutation Testing

Mutation testing (Stryker.NET) is manual and can be time-consuming. Install the tool once, then run it:

```bash
make install-quality-tools
make mutation
```

The mutation report is written to `StrykerOutput/**/reports/mutation-report.html`.

### Publishing Binaries

Produce self-contained, single-file executables for all supported platforms:

```bash
make publish
```

Or for a single runtime identifier:

```bash
make publish-linux-x64
```

---

## Know More

- [Project repository](https://github.com/amaurycarvalho/cobol-mutant-forge)
- [Releases](https://github.com/amaurycarvalho/cobol-mutant-forge/releases)
