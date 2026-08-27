## Context

The repository is currently a metadata-only shell (README, Makefile, CI, OpenSpec scaffolding) forked from an unrelated multi-service template. CobolMutantForge requires a fresh .NET 8.0 solution with a Clean Architecture + DDD layout matching the PRD (section 4.1). This change establishes that skeleton so later changes (domain, parser, plugins, mutation engine, export, CLI, tests) have a stable target to build against.

## Goals / Non-Goals

**Goals:**
- A buildable `CobolMutantForge.sln` with five projects in the correct layer topology.
- A single, shared `cobolmutantforge.json` configuration contract.
- Foundational package wiring (CLI framework, JSON serialization, logging, test framework).
- A clean directory layout that matches the PRD architecture diagram.

**Non-Goals:**
- Implementing any business logic (entities, mutations, parsing, plugins).
- Wiring the full CLI command set (deferred to `cli-commands`).
- Docker images or service decomposition (this is a single CLI tool, not a microservice stack).
- Release packaging (deferred to `adapt-project-metadata`).

## Decisions

- **Single solution, five projects** — matches the PRD's Clean Architecture decomposition and keeps the tool simple to build and test. Alternative considered: fewer projects (e.g., single assembly) rejected because the PRD explicitly mandates DDD layer separation.
- **.NET 8.0 LTS** — mandated by the PRD technology stack; LTS provides long-term stability for a freeware tool.
- **System.CommandLine** for the CLI shell — recommended by the PRD and provides built-in `--help`/`--version`/argument parsing.
- **System.Text.Json** for serialization — built-in, no extra dependency, matches the PRD.
- **Microsoft.Extensions.Logging + Serilog sink** — PRD lists both; Microsoft.Extensions.Logging is chosen as the abstraction with a console provider, allowing a future Serilog swap without changing call sites.
- **Configuration defined in the Infrastructure/Application boundary** — the DTO lives in Application, serialization in Infrastructure, mirroring the PRD's `JsonConfigSerializer`.

## Risks / Trade-offs

- [Scaffolding an empty skeleton could drift from the eventual feature implementation] → Keep the skeleton minimal and let feature changes own their logic.
- [Solution layout may need adjustment as TypeCobol integration lands] → Isolate parser integration in `type-cobol-parser` so bootstrap remains stable.
- [Versioning of the tool vs. the config file] → The tool version (0.1.0) and the config `version` field are distinct; only the tool version is governed by release tooling.

## Migration Plan

No migration — this is a greenfield scaffold. The template's stale files (Makefile, CI, README) are left untouched here and reconciled in `adapt-project-metadata`.
