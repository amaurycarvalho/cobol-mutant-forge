using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Interfaces;

public interface IMutationStrategy
{
    MutationType MutationType { get; }

    IReadOnlyList<Mutation> Apply(CobolProgram program);
}
