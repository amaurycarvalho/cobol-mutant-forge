using CobolMutantForge.Domain.Ast;

namespace CobolMutantForge.Infrastructure.Mutators;

internal static class AstTraversal
{
    public static IEnumerable<AstNode> AllNodes(AstNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in AllNodes(child))
            {
                yield return descendant;
            }
        }
    }

    public static string GetLine(string source, int lineNumber)
    {
        var lines = source.Split('\n');
        return lineNumber >= 1 && lineNumber <= lines.Length ? lines[lineNumber - 1] : string.Empty;
    }
}
