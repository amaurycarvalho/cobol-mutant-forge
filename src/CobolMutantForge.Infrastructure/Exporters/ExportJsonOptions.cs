using System.Text.Json;

namespace CobolMutantForge.Infrastructure.Exporters;

/// <summary>
/// Shared <see cref="JsonSerializerOptions"/> for the package export/import surface,
/// matching the camelCase structure published by the PRD.
/// </summary>
internal static class ExportJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
