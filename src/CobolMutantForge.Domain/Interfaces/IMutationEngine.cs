using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Interfaces;

public interface IMutationEngine
{
    IReadOnlyList<Mutation> GenerateMutations(
        CobolProgram program,
        MutationProfile profile,
        IReadOnlyList<TestCase>? testCases = null);

    bool ValidateMutation(CobolProgram program, Mutation mutation);

    string ApplyMutation(CobolProgram program, Mutation mutation);
}
