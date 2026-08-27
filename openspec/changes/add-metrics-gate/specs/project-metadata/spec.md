# project-metadata

## MODIFIED Requirements

### Requirement: Continuous integration

The `ci.yml` workflow SHALL run a single quality gate (restore, lint, test, coverage, metrics, security, and duplication) on pushes to `main` and pull requests, without a SonarCloud matrix or MCP integration-test job.

#### Scenario: CI pipeline

- **WHEN** the `ci.yml` workflow is inspected
- **THEN** it defines a quality-gate job for the single solution that runs the metrics and duplication checks, with no per-service or Docker-based jobs
