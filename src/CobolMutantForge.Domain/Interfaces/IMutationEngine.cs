using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Interfaces;

public interface IMutationEngine
{
    IReadOnlyList<Mutation> GenerateMutations(CobolProgram program, MutationProfile profile);
}
