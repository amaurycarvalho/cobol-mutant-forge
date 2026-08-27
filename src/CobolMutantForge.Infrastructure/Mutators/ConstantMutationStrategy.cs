using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Mutators;

/// <summary>
/// Placeholder for numeric/string constant mutation, planned for v2.0. It produces
/// no mutations yet; the profile matrix gates it so it only surfaces where the
/// corresponding profile flags are enabled.
/// </summary>
public sealed class ConstantMutationStrategy : IMutationStrategy
{
    public MutationType MutationType => MutationType.ConstantReplacement;

    public OperationType OperationType => OperationType.Constant;

    public IReadOnlyList<Mutation> Apply(CobolProgram program)
        => Array.Empty<Mutation>();
}
