using CobolMutantForge.Domain.Entities;

namespace CobolMutantForge.Application.Services;

/// <summary>
/// Validates that a <see cref="Mutation"/> is applicable to its program before it is
/// emitted: no-op mutations (mutated text equal to original) and mutations whose
/// original text is not present on the target line are rejected.
/// </summary>
public sealed class ValidationService
{
    public bool IsApplicable(CobolProgram program, Mutation mutation)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(mutation);

        if (string.Equals(mutation.Original, mutation.Mutated, StringComparison.Ordinal))
        {
            return false;
        }

        var lines = program.SourceText.Split('\n');
        if (mutation.Line < 1 || mutation.Line > lines.Length)
        {
            return false;
        }

        return lines[mutation.Line - 1].Contains(mutation.Original, StringComparison.Ordinal);
    }
}
