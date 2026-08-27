using CobolMutantForge.Domain.Interfaces;

namespace CobolMutantForge.Infrastructure.Plugins;

/// <summary>
/// Aggregate result of a ZUnit import: programs (from <c>.cbl</c>), test cases
/// (from <c>.xml</c>), the configuration (from <c>.bzucfg</c>), resolved copybooks,
/// warnings, and an overall validity flag.
/// </summary>
public sealed record ZUnitImportResult : ImportResult
{
    public ZUnitConfig Config { get; init; } = new();

    public IReadOnlyList<string> Copybooks { get; init; } = Array.Empty<string>();
}
