# Makefile
# Build/test/release tooling for the CobolMutantForge CLI.

.PHONY: install build test lint clean publish mutation install-quality-tools quality-gate help
.PHONY: $(addprefix publish-,$(RIDS))

# ---------- Variables ----------

# Single source of truth for the release version.
VERSION ?= 0.2.0

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
	@echo "$(GREEN)Running tests...$(NC)"
	@dotnet test "$(SOLUTION)" --no-restore -c $(CONFIG) || exit 1
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

mutation:
	@echo "$(GREEN)Running mutation tests (Stryker.NET, manual)...$(NC)"
	@dotnet-stryker --solution "$(SOLUTION)" --test-runner mtp || exit 1
	@echo "$(GREEN)Mutation tests complete$(NC)"

install-quality-tools:
	@echo "$(GREEN)Installing quality tools (dotnet-stryker)...$(NC)"
	@dotnet tool install --global dotnet-stryker || true
	@echo "$(GREEN)Quality tools installed$(NC)"

# Lean quality gate: lint + test. Mutation stays manual via `make mutation`.
quality-gate:
	@echo "$(GREEN)Running quality gate...$(NC)"
	@$(MAKE) lint
	@$(MAKE) test
	@echo "$(GREEN)All quality checks passed!$(NC)"

# ---------- Help ----------

help:
	@echo "$(GREEN)Available commands:$(NC)"
	@echo ""
	@echo "  make install    - Restore .NET dependencies"
	@echo "  make build      - Build $(SOLUTION) ($(CONFIG))"
	@echo "  make test       - Run the test suite"
	@echo "  make lint       - Verify formatting/analyzers (dotnet format --verify-no-changes)"
	@echo "  make clean      - Clean build artifacts and publish outputs"
	@echo "  make publish    - Publish self-contained single-file binaries for all RIDs"
	@echo "  make publish-<rid> - Publish a single RID (one of: $(RIDS))"
	@echo "  make mutation   - Run Stryker.NET mutation tests (manual, not in CI)"
	@echo "  make install-quality-tools - Install dotnet-stryker"
	@echo "  make quality-gate - Run the quality gate (lint + test)"
	@echo "  make help       - Show this help message"
