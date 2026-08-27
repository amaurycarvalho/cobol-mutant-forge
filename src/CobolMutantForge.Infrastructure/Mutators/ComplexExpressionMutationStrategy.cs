using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Mutators;

/// <summary>
/// Placeholder for complex-expression mutation, planned for v2.0. It produces no
/// mutations yet; the profile matrix gates it behind the complex-expressions flag.
/// </summary>
public sealed class ComplexExpressionMutationStrategy : IMutationStrategy
{
    public MutationType MutationType => MutationType.ComplexExpressionMutation;

    public OperationType OperationType => OperationType.ComplexExpression;

    public IReadOnlyList<Mutation> Apply(CobolProgram program)
        => Array.Empty<Mutation>();
}
