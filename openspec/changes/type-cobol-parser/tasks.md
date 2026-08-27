## 1. Dependency and adapter skeleton

- [ ] 1.1 Add the TypeCobol dependency to `CobolMutantForge.Infrastructure`
- [ ] 1.2 Create `Parsers/TypeCobolParserAdapter.cs` implementing `ICobolParser`
- [ ] 1.3 Define the parse result type (AST + diagnostics) used by the adapter

## 2. AST mapping

- [ ] 2.1 Map TypeCobol parse output onto the domain `AstNode` model
- [ ] 2.2 Emit nodes for logical operators (`AND`, `OR`, `NOT`)
- [ ] 2.3 Emit nodes for arithmetic operators (`+`, `-`, `*`, `/`)
- [ ] 2.4 Exclude operators inside comments and string literals

## 3. Diagnostics

- [ ] 3.1 Surface parser errors with line/column information
- [ ] 3.2 Return warnings for unsupported constructs instead of hard failures

## 4. Verification

- [ ] 4.1 Parse a representative `IF ... AND ...` program and assert the `AND` node is discovered
- [ ] 4.2 Parse a `COMPUTE ... + ...` program and assert the `+` node is discovered
- [ ] 4.3 Parse a program with commented operators and assert they are excluded
