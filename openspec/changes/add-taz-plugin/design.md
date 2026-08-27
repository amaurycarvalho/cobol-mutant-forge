## Context

`TestAcceleratorPlugin` is an inert stub (`PluginBase + IImportPlugin + IExportPlugin`, `Version "2.0.0"`, throwing `NotSupportedException`), registered in `ServiceCollectionExtensions` and surfaced by `plugin list`. The codebase is fully synchronous (`IImportPlugin.Import(string)` → `ImportResult`; `IExportPlugin.Export(MutantPackage, string)`), tolerant by design (parsers never throw; they record warnings), and has no YAML/REST/process dependencies. `MutationConfigDto.TestAccelerator` is currently an untyped `Dictionary<string, object?>`.

RFC-003 describes a full TAZ integration (async interfaces, `taz` CLI executor, ODE REST, code coverage collector, `.ztest` reverse-engineering, `zapp.yaml`). Most of it requires a z/OS runtime an offline mutation CLI cannot reach. This design scopes the change to consuming TAZ's JSON/file artifacts and typing the config, while introducing the runtime surfaces as inert ports.

## Goals / Non-Goals

**Goals:**

- Make `TestAcceleratorPlugin` a functional importer/exporter for `zapp.json`, `.zdata` (JSON), and results (JUnit/JSON).
- Type `TazPluginConfiguration` and wire it into `MutationConfigDto` with empty defaults.
- Introduce `ITazCliExecutor`, `ITazRestApiClient`, `ICodeCoverageCollector` as ports with "not yet supported" stubs.

**Non-Goals:**

- No `.ztest` parsing (proprietary/undocumented) — stub + warning.
- No `zapp.yaml` support (no YAML dependency) — JSON only.
- No ODE REST, code coverage, or z/OS file-system integration.
- No `IExecutionPlugin`/`IValidationPlugin`/`ICoveragePlugin` contracts.
- No async/request-response signatures; the existing synchronous plugin contracts are retained.

## Decisions

### 1. Retain synchronous plugin contracts

The TAZ plugin implements the existing `IImportPlugin`/`IExportPlugin` synchronously. The RFC's `async Task` + `*Request`/`*Result` DTOs are rejected: the whole pipeline (command → use case → plugin) is synchronous, and async would ripple through every layer for no current benefit.

**Alternatives considered:** introduce async variants of the port interfaces — rejected, inconsistent and unused while runtime is stubbed.

### 2. Import scope = JSON/file artifacts only

`TazProjectConfigParser` (JSON via `System.Text.Json`), `ZDataParser` (`.zdata` JSON), and `TazResultParser` (JUnit XML + JSON output) follow the tolerant `Parse(string) → ParseResult { Payload, Warnings, IsValid }` house pattern. `.ztest` and `zapp.yaml` are recognized but produce a "not yet supported" warning.

**Alternatives considered:** add a YAML library for `zapp.yaml` — rejected (new dependency; user decision: JSON-only); reverse-engineer `.ztest` — rejected (no public spec or samples; user decision: stub).

### 3. Typed `TazPluginConfiguration` with empty defaults

`MutationConfigDto.TestAccelerator` becomes `TazPluginConfiguration` (record, `init`-style, defaults empty), replacing the `Dictionary<string, object?>`. Existing generated configs remain valid because the typed defaults are empty/backward compatible.

**Alternatives considered:** keep the dictionary and add a typed section alongside — rejected, leaves two sources of truth; introduce a breaking config schema — rejected (user decision: empty defaults).

### 4. Runtime ports created even while stubbed

`ITazCliExecutor`, `ITazRestApiClient`, `ICodeCoverageCollector` are declared now with "not yet supported" implementations mirroring the current stub, fixing the future contract without pulling in network/process concerns.

**Alternatives considered:** defer the ports until a real z/OS target exists — rejected (user decision: create stubbed ports).

### 5. Reuse framework-neutral packaging from `expand-zunit-plugin`

Export delegates to the same manifest-v2/coverage-map structures introduced for ZUnit; the TAZ change consumes them rather than redefining. Build order: `expand-zunit-plugin` first.

## Risks / Trade-offs

- **Proprietary results XML drift** → Mitigation: JUnit/JSON are the primary, well-documented formats; native XML is parsed tolerantly.
- **Stubs mistaken for support** → Mitigation: explicit "not yet supported" messages and import warnings.
- **Typed config vs. generated JSON** → Mitigation: empty defaults keep old `cobolmutantforge.json` files valid.
- **Cross-change dependency** → Mitigation: document that `add-taz-plugin` follows `expand-zunit-plugin`; reuse, don't redefine.
- **`.ztest`/`zapp.yaml` gaps limit coverage** → Mitigation: documented as deferred; import warns instead of failing.
