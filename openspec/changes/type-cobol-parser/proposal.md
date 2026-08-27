## Why

The mutation engine needs a structured representation of COBOL source code to locate operators safely (rather than naive text matching, which would mutate comments and string literals). The PRD mandates TypeCobol as the COBOL parser. This change introduces the parser adapter that turns COBOL source into the domain's AST abstraction.

## What Changes

- Add the TypeCobol dependency to the Infrastructure project.
- Implement `TypeCobolParserAdapter` conforming to `ICobolParser`.
- Map TypeCobol's parse tree onto the domain's minimal `AstNode` representation.
- Surface parser diagnostics (errors/warnings) to callers.
- Provide a fallback-friendly error path for unsupported constructs.

## Capabilities

### New Capabilities
- `cobol-parsing`: Parsing COBOL source into the domain AST representation via TypeCobol.

## Impact

- Populates `src/CobolMutantForge.Infrastructure/Parsers/`.
- Adds the TypeCobol NuGet/GitHub dependency.
- The `ICobolParser` port is now realized; `zunit-plugin` and `mutation-engine` consume it.
