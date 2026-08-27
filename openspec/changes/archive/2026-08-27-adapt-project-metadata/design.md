## Context

The repository was initialized from a multi-service template and its metadata still names `agentic-fp-ai-mvp`, Docker images, per-service SonarCloud, and MCP integration tests. CobolMutantForge is a single freeware CLI tool. This change rewrites the build/test/release/docs tooling to match that reality.

## Goals / Non-Goals

**Goals:**
- A `Makefile` and CI that build/test/lint a single solution.
- A release pipeline that produces downloadable, self-contained executables for personal use.
- English-only, accurate documentation and changelogs.
- Release skills that reflect a single CLI tool.

**Non-Goals:**
- Docker packaging, container registries, or service orchestration.
- Self-hosted SonarQube / SonarCloud matrices.
- Changing the application source (owned by the feature changes).

## Decisions

- **`VERSION ?= 0.1.0` in the Makefile as the single source of truth** — the release skills and workflows derive the version from it, preserving the existing convention.
- **Self-contained, single-file publish per RID** (`win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`) — the best fit for "easily download and install for personal use": users get a runnable binary with no .NET SDK. Alternatives considered: (a) framework-dependent publish (smaller but requires SDK/runtime — rejected); (b) .NET global tool via `dotnet tool install` (convenient for developers but requires the SDK — rejected as the *primary* artifact, may be added later).
- **Archive per platform** — `.zip` for Windows, `.tar.gz` otherwise, attached via `softprops/action-gh-release`.
- **CI quality gate = restore + format-check + build + test** — enough for a single tool; Stryker mutation remains a manual `make mutation` step.
- **Remove the SonarQube compose stack** — self-hosting Sonar is irrelevant for a freeware CLI; the `sonarqube/` directory and its Makefile targets are dropped.

## Risks / Trade-offs

- [Self-contained binaries are larger] → Acceptable for a CLI; single-file publish keeps distribution simple.
- [Cross-platform publish needs to run on a Linux runner with per-RID targets] → `dotnet publish -r <rid> --self-contained true /p:PublishSingleFile=true` produces all RIDs from the Ubuntu runner.
- [Signing/notarization on macOS/Windows is out of scope] → Document that users may need to trust unsigned binaries (acceptable for freeware).

## Migration Plan

1. Rewrite `Makefile`, `README.md`, and workflows.
2. Reset the changelogs to a `0.1.0` baseline.
3. Update the release skills.
4. Delete `sonarqube/` and prune `.gitignore`.
5. Validate `make build test` and a dry-run of the release workflow.
