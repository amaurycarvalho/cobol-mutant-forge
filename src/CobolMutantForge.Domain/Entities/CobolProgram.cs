using System.Security.Cryptography;
using System.Text;
using CobolMutantForge.Domain.Ast;

namespace CobolMutantForge.Domain.Entities;

public sealed class CobolProgram
{
    public string Name { get; }
    public string SourceText { get; }
    public IReadOnlyList<string> Copybooks { get; }
    public AstNode? Ast { get; }
    public string SourceHash { get; }

    public CobolProgram(string name, string sourceText, IReadOnlyList<string>? copybooks = null, AstNode? ast = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(sourceText);

        Name = name;
        SourceText = sourceText;
        Copybooks = copybooks ?? Array.Empty<string>();
        Ast = ast;
        SourceHash = ComputeHash(sourceText);
    }

    public override bool Equals(object? obj) => Equals(obj as CobolProgram);

    public bool Equals(CobolProgram? other)
        => other is not null && Name == other.Name && SourceHash == other.SourceHash;

    public override int GetHashCode() => HashCode.Combine(Name, SourceHash);

    private static string ComputeHash(string sourceText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sourceText));
        return Convert.ToHexString(hash);
    }
}
