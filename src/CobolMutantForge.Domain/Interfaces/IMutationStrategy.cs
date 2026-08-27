using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Interfaces;

public interface IMutationStrategy
{
    MutationType MutationType { get; }

    OperationType OperationType { get; }

    IReadOnlyList<Mutation> Apply(CobolProgram program);
}
