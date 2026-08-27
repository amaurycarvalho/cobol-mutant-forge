# Makefile
# Build/test/release tooling for the CobolMutantForge CLI.

.PHONY: install build test lint clean publish metrics coverage coverage-check security mutation install-quality-tools quality-gate duplication help
.PHONY: $(addprefix publish-,$(RIDS))

# ---------- Variables ----------

# Single source of truth for the release version.
VERSION ?= 0.3.0

# Coverage floor (constitution requires >=90% for low-criticality code and
# >=100% for medium/high criticality). Overall measured: ~98% with real tests
# on agent/mcp/rag. Tighten further as more logic lands.
COVERAGE_THRESHOLD ?= 90

# Build configuration.
CONFIG ?= Release

# Root solution.
SOLUTION := CobolMutantForge.sln

# Runtime identifiers published as self-contained single-file binaries.
RIDS := win-x64 linux-x64 osx-x64 osx-arm64

# Publish output directories.
PUBLISH_DIR := artifacts/publish
RELEASE_DIR := artifacts/release

# Output colors.
GREEN := \033[0;32m
YELLOW := \033[0;33m
NC := \033[0m

# ---------- Main commands ----------

install:
	@echo "$(GREEN)Restoring .NET dependencies...$(NC)"
	@dotnet restore "$(SOLUTION)" || exit 1
	@echo "$(GREEN)Restore complete$(NC)"

build:
	@echo "$(GREEN)Building $(SOLUTION) ($(CONFIG))...$(NC)"
	@dotnet build "$(SOLUTION)" -c $(CONFIG) || exit 1
	@echo "$(GREEN)Build complete$(NC)"

test:
	@echo "$(GREEN)Running tests with coverage...$(NC)"
	@dotnet test "$(SOLUTION)" --no-restore -c $(CONFIG) \
		--collect:"XPlat Code Coverage" \
		--results-directory TestResults || exit 1
	@echo "$(GREEN)Tests passed$(NC)"

lint:
	@echo "$(GREEN)Lint (dotnet format --verify-no-changes)...$(NC)"
	@dotnet format "$(SOLUTION)" --verify-no-changes || exit 1
	@echo "$(GREEN)Lint passed$(NC)"

clean:
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	@dotnet clean "$(SOLUTION)" || exit 1
	@find . -type d \( -name bin -o -name obj \) -not -path './.git/*' -prune -exec rm -rf {} + 2>/dev/null || true
	@rm -rf $(PUBLISH_DIR) $(RELEASE_DIR) TestResults
	@echo "$(GREEN)Clean complete$(NC)"

# ---------- Publish targets ----------
# Produce self-contained, single-file executables for each runtime identifier.

publish: $(addprefix publish-,$(RIDS))

publish-%:
	@rid="$(@:publish-%=%)"; \
	echo "$(GREEN)Publishing $$rid (self-contained single-file)...$(NC)"; \
	dotnet publish src/CobolMutantForge.CLI/CobolMutantForge.CLI.csproj \
		-c $(CONFIG) \
		-r $$rid \
		--self-contained true \
		-p:PublishSingleFile=true \
		-p:Version=$(VERSION) \
		-o "$(RELEASE_DIR)/$$rid" || exit 1; \
	echo "$(GREEN)Publish complete: $$rid$(NC)"

# ---------- Quality targets ----------

metrics:
	@echo "$(GREEN)Code metrics (Lines of Code)...$(NC)"
	@for proj in src/*/; do \
		count="$$(find "$$proj" -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' -exec cat {} + 2>/dev/null | wc -l)"; \
		echo "  $$(basename "$$proj"): $$count LOC"; \
	done
	@echo "$(GREEN)Metrics complete$(NC)"

coverage: test coverage-check

coverage-check:
	@echo "$(GREEN)📊 Checking coverage against threshold (>= $(COVERAGE_THRESHOLD)%)...$(NC)"
	@python3 scripts/coverage_check.py "$(COVERAGE_THRESHOLD)"

security:
	@echo "$(GREEN)Security scan (dependencies + SAST)...$(NC)"
	@for proj in src/*/*.csproj; do \
		dir="$$(dirname "$$proj")"; \
		echo "  -> $$proj --vulnerable"; \
		out="$$(cd "$$dir" && dotnet list package --vulnerable 2>&1)"; \
		echo "$$out"; \
		if echo "$$out" | grep -qi "vulnerab" && ! echo "$$out" | grep -qi "no vulnerable packages"; then \
			echo "$(RED)Vulnerable packages found$(NC)"; exit 1; \
		fi; \
		echo "  -> $$proj --deprecated"; \
		(cd "$$dir" && dotnet list package --deprecated); \
		echo "  -> $$proj --outdated"; \
		(cd "$$dir" && dotnet list package --outdated); \
	done
	@echo "  -> semgrep (C# source)..."
	@if command -v semgrep >/dev/null 2>&1; then \
		semgrep ci --oss-only --quiet --config auto --include '*.cs' || exit 1; \
	else \
		echo "$(YELLOW)semgrep not found - skipping SAST scan$(NC)"; \
	fi
	@echo "$(GREEN)Security scan complete$(NC)"

duplication:
	@echo "$(GREEN)Duplication check (jscpd, threshold 10%)...$(NC)"
	@if command -v jscpd >/dev/null 2>&1; then \
		jscpd src --format csharp --threshold 10 \
			--ignore "**/obj/**,**/bin/**" \
			--reporters console,json || exit 1; \
	else \
		echo "$(RED)jscpd not found. Install it first (e.g. 'npm install -g jscpd').$(NC)"; \
		exit 1; \
	fi
	@echo "$(GREEN)Duplication check passed (<= 10%)$(NC)"

mutation:
	@echo "$(GREEN)Running mutation tests (Stryker.NET, manual)...$(NC)"
	@dotnet-stryker --solution "$(SOLUTION)" --test-runner mtp || exit 1
	@echo "$(GREEN)Mutation tests complete$(NC)"

install-quality-tools:
	@echo "$(GREEN)Installing quality tools (dotnet-stryker)...$(NC)"
	@dotnet tool install --global dotnet-stryker || true
	@echo "$(GREEN)Quality tools installed$(NC)"

# Lean quality gate: lint + test + coverage + metrics + duplication. Mutation stays manual via `make mutation`.
quality-gate:
	@echo "$(GREEN)Running quality gate...$(NC)"
	@$(MAKE) lint
	@$(MAKE) test
	@$(MAKE) coverage-check
	@$(MAKE) metrics
	@$(MAKE) duplication
	@$(MAKE) security
	@echo "$(GREEN)All quality checks passed!$(NC)"

# ---------- Help ----------

help:
	@echo "$(GREEN)Available commands:$(NC)"
	@echo ""
	@echo "  make install    - Restore .NET dependencies"
	@echo "  make build      - Build $(SOLUTION) ($(CONFIG))"
	@echo "  make test       - Run the test suite"
	@echo "  make lint       - Verify formatting/analyzers (dotnet format --verify-no-changes)"
	@echo "  make metrics          - Report Lines of Code per source project"
	@echo "  make coverage         - Run tests and check coverage threshold"
	@echo "  make coverage-check   - Check coverage against COVERAGE_THRESHOLD (default 90)"
	@echo "  make security         - Check package vulnerabilities/deprecated/outdated + Semgrep SAST"
	@echo "  make duplication      - Check code duplication (jscpd, threshold 10%)"
	@echo "  make clean      - Clean build artifacts and publish outputs"
	@echo "  make publish    - Publish self-contained single-file binaries for all RIDs"
	@echo "  make publish-<rid> - Publish a single RID (one of: $(RIDS))"
	@echo "  make mutation   - Run Stryker.NET mutation tests (manual, not in CI)"
	@echo "  make install-quality-tools - Install dotnet-stryker"
	@echo "  make quality-gate - Run the quality gate (lint + test + coverage + metrics + duplication + security)"
	@echo "  make help       - Show this help message"
