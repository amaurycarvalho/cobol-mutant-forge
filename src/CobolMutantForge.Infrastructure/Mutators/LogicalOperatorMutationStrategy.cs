using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Mutators;

/// <summary>
/// Mutates logical operators: <c>AND</c> ↔ <c>OR</c>, removal of <c>NOT</c>, and
/// insertion of <c>NOT</c> into non-negated <c>IF</c> conditions (the "where
/// applicable" case from the spec). The mutation line is the AST node's line; text
/// replacement is line-scoped via <see cref="Mutation.Original"/> /
/// <see cref="Mutation.Mutated"/>.
/// </summary>
public sealed class LogicalOperatorMutationStrategy : IMutationStrategy
{
    public MutationType MutationType => MutationType.AndToOr;

    public OperationType OperationType => OperationType.Logical;

    public IReadOnlyList<Mutation> Apply(CobolProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        if (program.Ast is null)
        {
            return Array.Empty<Mutation>();
        }

        var mutations = new List<Mutation>();
        var index = 0;
        foreach (var node in AstTraversal.AllNodes(program.Ast))
        {
            if (node.Kind != "LogicalOperator")
            {
                continue;
            }

            switch (node.Text.ToUpperInvariant())
            {
                case "AND":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.AndToOr, node.Line, "AND", "OR"));
                    AddNotInsertion(program, node, mutations);
                    break;
                case "OR":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.OrToAnd, node.Line, "OR", "AND"));
                    AddNotInsertion(program, node, mutations);
                    break;
                case "NOT":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.RemoveNot, node.Line, "NOT", string.Empty));
                    break;
            }
        }

        return mutations;
    }

    private static void AddNotInsertion(CobolProgram program, AstNode node, List<Mutation> mutations)
    {
        var line = AstTraversal.GetLine(program.SourceText, node.Line);
        if (!line.TrimStart().StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (line.Contains("NOT", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        mutations.Add(new Mutation("tmp-not", MutationType.AddNot, node.Line, "IF ", "IF NOT "));
    }
}
