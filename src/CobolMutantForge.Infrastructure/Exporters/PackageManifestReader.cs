using System.Text.Json;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Exporters;

/// <summary>
/// Reconstructs <see cref="MutantPackage"/> instances from an exported directory of
/// mutants (the <c>--source</c> of the export command). Each package is a subfolder
/// containing a <c>manifest.json</c> and the original <c>.cbl</c> source; mutations
/// are rebuilt from the manifest entries so the folder can be re-packaged as zip or
/// folder by <see cref="MutantPackageExporter"/>.
/// </summary>
public sealed class PackageManifestReader
{
    public IReadOnlyList<MutantPackage> ReadAll(string sourceDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);

        var packages = new List<MutantPackage>();
        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var manifestPath = Path.Combine(directory, "manifest.json");
            if (File.Exists(manifestPath))
            {
                packages.Add(Read(manifestPath));
            }
        }

        return packages;
    }

    public MutantPackage Read(string manifestPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<ManifestDto>(json, ExportJsonOptions.Default)
            ?? throw new InvalidDataException("Failed to deserialize the manifest.");

        var directory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
        var program = LoadSourceProgram(directory, manifest.OriginalProgram);

        var package = new MutantPackage(manifest.MutantId, program)
        {
            Profile = MutationProfile.FromName(manifest.MutationProfile)
        };
        foreach (var entry in manifest.Mutations)
        {
            package.AddMutant(new Mutation(
                entry.Id,
                ToMutationType(entry.Type),
                entry.Line,
                entry.Original,
                entry.Mutated,
                entry.TestCaseCoverage));
        }

        return package;
    }

    private static CobolProgram? LoadSourceProgram(string directory, string programName)
    {
        var sourcePath = Path.Combine(directory, $"{programName}.cbl");
        if (!File.Exists(sourcePath))
        {
            return null;
        }

        return new CobolProgram(programName, File.ReadAllText(sourcePath));
    }

    private static MutationType ToMutationType(string type)
        => type.ToLowerInvariant() switch
        {
            "logical_operator" => MutationType.AndToOr,
            "arithmetic_operator" => MutationType.AddToSubtract,
            "constant_replacement" => MutationType.ConstantReplacement,
            "complex_expression" => MutationType.ComplexExpressionMutation,
            _ => MutationType.ConstantReplacement
        };
}
