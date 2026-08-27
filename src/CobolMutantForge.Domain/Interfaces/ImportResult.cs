using CobolMutantForge.Domain.Entities;

namespace CobolMutantForge.Domain.Interfaces;

public record ImportResult
{
    public IReadOnlyList<CobolProgram> Programs { get; init; } = Array.Empty<CobolProgram>();
    public IReadOnlyList<TestCase> TestCases { get; init; } = Array.Empty<TestCase>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public bool IsValid { get; init; } = true;
}
