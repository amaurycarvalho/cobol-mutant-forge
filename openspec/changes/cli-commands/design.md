## Context

All pipeline pieces (import, parse, mutate, export) now exist behind domain interfaces. The CLI is the composition root that binds them via DI and exposes the PRD command surface. System.CommandLine provides argument parsing and built-in help/version.

## Goals / Non-Goals

**Goals:**
- A runnable `cobol-mutant-forge` executable with `init`, `generate`, `export`, `plugin list`, `--help`, and `--version`.
- Clean DI wiring via `ServiceCollectionExtensions`.
- Consistent `--quiet` behavior.

**Non-Goals:**
- Interactive mode (PRD future feature).
- GUI or any non-CLI surface.

## Decisions

- **System.CommandLine** — recommended by the PRD; gives typed options and auto-generated help.
- **One command class per PRD command** — `InitCommand`, `GenerateCommand`, `ExportCommand`, `PluginCommand`, matching the PRD's CLI folder layout.
- **DI composition root in the CLI project** — the CLI references Application and Infrastructure and registers the parser adapter, ZUnit plugin, mutation engine, and exporters.
- **`--version` reads the assembly informational version** — single source of truth aligned with the release tooling (0.1.0).

## Risks / Trade-offs

- [Command surface may evolve] → Keep commands thin (delegate to use cases) so changes stay localized.
- [Version mismatch between CLI and release tag] → Centralize version in one place (assembly info driven by the release tooling).

## Migration Plan

None — additive. This is the first user-facing surface.
