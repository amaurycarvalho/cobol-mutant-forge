using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Mutators;

/// <summary>
/// Mutates arithmetic operators: <c>+</c> ↔ <c>-</c> and <c>*</c> ↔ <c>/</c>,
/// matching the operators surfaced by the parser adapter as
/// <see cref="AstNode"/> nodes of kind <c>ArithmeticOperator</c>.
/// </summary>
public sealed class ArithmeticOperatorMutationStrategy : IMutationStrategy
{
    public MutationType MutationType => MutationType.AddToSubtract;

    public OperationType OperationType => OperationType.Arithmetic;

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
            if (node.Kind != "ArithmeticOperator")
            {
                continue;
            }

            switch (node.Text)
            {
                case "+":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.AddToSubtract, node.Line, "+", "-"));
                    break;
                case "-":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.SubtractToAdd, node.Line, "-", "+"));
                    break;
                case "*":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.MultiplyToDivide, node.Line, "*", "/"));
                    break;
                case "/":
                    mutations.Add(new Mutation($"tmp-{index++}", MutationType.DivideToMultiply, node.Line, "/", "*"));
                    break;
            }
        }

        return mutations;
    }
}
