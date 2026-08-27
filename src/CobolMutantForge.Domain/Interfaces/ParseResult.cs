using CobolMutantForge.Domain.Ast;

namespace CobolMutantForge.Domain.Interfaces;

public sealed record ParseResult
{
    public AstNode Ast { get; init; } = new();
    public IReadOnlyList<ParseDiagnostic> Diagnostics { get; init; } = Array.Empty<ParseDiagnostic>();
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
