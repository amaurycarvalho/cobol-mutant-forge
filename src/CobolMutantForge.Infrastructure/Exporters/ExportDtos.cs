namespace CobolMutantForge.Infrastructure.Exporters;

/// <summary>One mutation entry shared by the manifest and the mutations report.</summary>
public sealed record MutationEntryDto
{
    public string Id { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Line { get; init; }

    public string Original { get; init; } = string.Empty;

    public string Mutated { get; init; } = string.Empty;

    public IReadOnlyList<string> TestCaseCoverage { get; init; } = Array.Empty<string>();
}

/// <summary>
/// The <c>manifest.json</c> structure defined by the PRD (section 5.3.4).
/// Serialized with camelCase property naming to match the published format.
/// </summary>
public sealed record ManifestDto
{
    public string MutantId { get; init; } = string.Empty;

    public string OriginalProgram { get; init; } = string.Empty;

    public string BaseProgramHash { get; init; } = string.Empty;

    public DateTimeOffset Timestamp { get; init; }

    public string MutationProfile { get; init; } = string.Empty;

    public IReadOnlyList<MutationEntryDto> Mutations { get; init; } = Array.Empty<MutationEntryDto>();

    public bool SourceCopied { get; init; }

    public bool CopybooksResolved { get; init; }
}

/// <summary>The <c>mutations-report.json</c> structure enumerating every applied mutation.</summary>
public sealed record MutationsReportDto
{
    public string MutantId { get; init; } = string.Empty;

    public string OriginalProgram { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }

    public int TotalMutations { get; init; }

    public IReadOnlyList<MutationEntryDto> Mutations { get; init; } = Array.Empty<MutationEntryDto>();
}
