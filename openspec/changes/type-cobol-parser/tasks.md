## 1. Dependency and adapter skeleton

- [x] 1.1 Add the TypeCobol dependency to `CobolMutantForge.Infrastructure`
- [x] 1.2 Create `Parsers/TypeCobolParserAdapter.cs` implementing `ICobolParser`
- [x] 1.3 Define the parse result type (AST + diagnostics) used by the adapter

## 2. AST mapping

- [x] 2.1 Map TypeCobol parse output onto the domain `AstNode` model
- [x] 2.2 Emit nodes for logical operators (`AND`, `OR`, `NOT`)
- [x] 2.3 Emit nodes for arithmetic operators (`+`, `-`, `*`, `/`)
- [x] 2.4 Exclude operators inside comments and string literals

## 3. Diagnostics

- [x] 3.1 Surface parser errors with line/column information
- [x] 3.2 Return warnings for unsupported constructs instead of hard failures

## 4. Verification

- [x] 4.1 Parse a representative `IF ... AND ...` program and assert the `AND` node is discovered
- [x] 4.2 Parse a `COMPUTE ... + ...` program and assert the `+` node is discovered
- [x] 4.3 Parse a program with commented operators and assert they are excluded
