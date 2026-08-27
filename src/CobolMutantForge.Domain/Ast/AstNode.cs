namespace CobolMutantForge.Domain.Ast;

public sealed record AstNode
{
    public string Kind { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<AstNode> Children { get; init; } = Array.Empty<AstNode>();
}
