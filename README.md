# cobol-mutant-forge

A Mutation Testing Tool for COBOL/CICS Programs.

[![Spec-Driven Development](https://img.shields.io/badge/SDD-OpenSpec-yellow)](openspec/specs/project-constitution/spec.md)

---

## 🧑‍💻 For Users

### Features

### Requirements


### How to Install


### How to Use


---

## 👨‍🔧 For Developers

### Specifications

This project uses [Spec-Driven Development (SDD)](https://opencode.ai). All specifications live under [`openspec/specs/`](openspec/specs/):

Active changes are tracked under [`openspec/changes/`](openspec/changes/).

Archived changes are tracked under [`openspec/changes/archive/`](openspec/changes/archive/).

### How to Get the Source Code

```bash
git clone https://github.com/amaurycarvalho/cobol-mutant-forge.git
```

### How to Install and Build

```bash
make install
make build
```

Requirements:

- .NET SDK 8.0
- VSCode
- OpenSpec
- Stryker.Net

#### Linting and Unit Testing

```bash
make lint test
```

#### Quality Gate

The quality gate executes linting, tests (with coverage), coverage verification,
metrics, and security checks:

```bash
make quality-gate
```

Individual checks:

```bash
make lint               # formatting/analysis (dotnet format --verify-no-changes)
make test               # tests + coverage
make coverage-check     # coverage against COVERAGE_THRESHOLD (default 80)
make metrics            # lines of code (LOC) per service
make security           # vulnerable/deprecated/outdated packages + Semgrep SAST
```

Static analysis, complexity, code smells, technical debt, and maintainability
ratings are managed by **SonarCloud** within the CI pipeline, featuring
per-service analysis, a *Leak Period* for new code, and Pull Request decoration.
Coverage data is reported via `TestResults/**/coverage.cobertura.xml`.

> **CI Jobs:** the `sonarcloud` job (SonarCloud) and the `integration-test`
> job run **only on pull requests**.
> The `quality-gate` job (lint + test + coverage + metrics + security)
> runs on pushes to `main` and on pull requests.

#### SonarCloud analysis in the CI pipeline

SonarCloud requires the following secrets to be configured in GitHub:

```
SONAR_PROJECT_KEY
SONAR_ORG
SONAR_TOKEN
```

#### Local SonarQube analysis (self-hosted)

To analyze services locally against a running **self-hosted SonarQube**
server (e.g., `http://localhost:9000`), install the scanner and
run the analysis for each service:

```bash
make sonar-install
SONAR_TOKEN=<your-token> make sonar-check
```

The `sonar-check` command executes the sequence `begin → build + test (with coverage) → end`.

##### Spinning up a local SonarQube server (Docker Compose)

The repository includes a reproducible local stack (SonarQube Community +
PostgreSQL, with persistent volumes) located at `sonarqube/docker-compose.yml`.
It is based on the official SonarSource reference and incorporates the same
hardening measures (`read_only`, `tmpfs`, named volumes). Full workflow:

```bash
make sonar-up        # starts the stack and waits for SonarQube to be ready
# 1) Access http://localhost:9000 and log in with admin / admin
# 2) Change the password on first login (mandatory)
# 3) My Account -> Security -> Tokens -> Generate (admin user token)
SONAR_TOKEN=<your-token> make sonar-check   # analyzes the 4 services
make sonar-down      # stops the stack while preserving volumes
```

With an **admin** user token, the four per-service projects (the keys
displayed by `make sonar-up`, one per service) are **automatically created**
during the first analysis.

**Host requirements:**

- **Linux:** the embedded Elasticsearch requires a higher `vm.max_map_count`; apply
`sudo sysctl -w vm.max_map_count=262144` (make it persistent in `/etc/sysctl.conf`).
- **Docker Desktop (Windows/Mac):** allocate at least 2–4 GB of memory to the
engine (the compose file sets `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true` to avoid
`max_map_count` failures, as this setting isn't directly configurable on these hosts).
- **Full reset** (deletes stack data): `docker compose -f sonarqube/docker-compose.yml down -v`.
- The `admin`/`admin` credentials are for local development only — do not use
them in production.

Environment variables:

- `SONAR_HOST_URL` — SonarQube server URL (default `http://localhost:9000`);
- `SONAR_TOKEN` — authentication token (mandatory);
- `SONAR_PROJECT_KEY` — project key.

> The local scanner state (`/.sonarqube`) is ignored by git. The local analysis
> uses the same Cobertura coverage reports (`TestResults/**/coverage.cobertura.xml`)
> as `make test`, excluding test sources.

### Mutation testing

Make sure everything is installed.

```bash
make install-quality-tools
```

Run it locally (it can be time-consuming and require significant processing).

```bash
make mutation
```

Then, get `services/**/tests/**/StrykerOutput/**/reports/mutation-report.json` and `services/**/tests/**/StrykerOutput/**/reports/mutation-report.html` files and use it with your AI agent to fix your unit tests.

Finally, run the mutation testing again and check if it pass the quality gate.

---

## Know More

- [Project repository](https://github.com/amaurycarvalho/cobol-mutant-forge)
- [Releases](https://github.com/amaurycarvalho/cobol-mutant-forge/releases)

