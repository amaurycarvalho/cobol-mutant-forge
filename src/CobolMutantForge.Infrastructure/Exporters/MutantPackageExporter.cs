using System.IO.Compression;
using System.Text;
using System.Text.Json;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Mutators;

namespace CobolMutantForge.Infrastructure.Exporters;

/// <summary>
/// Realizes <see cref="IExportPlugin"/>, packaging a <see cref="MutantPackage"/> into
/// the published objects for CICS import: mutated <c>.cbl</c> sources, the original
/// source, <c>manifest.json</c>, and <c>mutations-report.json</c>. Supports both
/// <see cref="ExportFormat.Folder"/> and <see cref="ExportFormat.Zip"/> output; both
/// share the same file-assembly logic and the zip streams the assembled files directly
/// into the archive.
/// </summary>
public sealed class MutantPackageExporter : IExportPlugin
{
    private const int PackageSequence = 1;

    private readonly IMutationEngine _engine;
    private readonly ExportFormat _format;

    public MutantPackageExporter(ExportFormat format = ExportFormat.Folder, IMutationEngine? engine = null)
    {
        _format = format;
        _engine = engine ?? new MutationEngine();
    }

    public string Name => "mutant-package";

    public void Export(MutantPackage package, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var files = AssembleFiles(package);
        if (_format == ExportFormat.Zip)
        {
            WriteZip(package, outputDirectory, files);
        }
        else
        {
            WriteFolder(package, outputDirectory, files);
        }

        package.Manifest = JsonSerializer.Serialize(BuildManifest(package), ExportJsonOptions.Default);
        package.Report = JsonSerializer.Serialize(BuildReport(package), ExportJsonOptions.Default);
    }

    private IReadOnlyList<(string Path, string Content)> AssembleFiles(MutantPackage package)
    {
        var program = package.SourceProgram;
        var programName = program?.Name ?? package.Id;
        var files = new List<(string, string)>();

        if (program is not null)
        {
            files.Add(($"{programName}.cbl", program.SourceText));
            var mutations = package.Mutants;
            for (var index = 0; index < mutations.Count; index++)
            {
                var fileName = $"{BuildMutantId(programName, index + 1)}.cbl";
                files.Add((fileName, _engine.ApplyMutation(program, mutations[index])));
            }
        }

        files.Add(("manifest.json", JsonSerializer.Serialize(BuildManifest(package), ExportJsonOptions.Default)));
        files.Add(("mutations-report.json", JsonSerializer.Serialize(BuildReport(package), ExportJsonOptions.Default)));
        return files;
    }

    private static void WriteFolder(
        MutantPackage package, string outputDirectory, IReadOnlyList<(string Path, string Content)> files)
    {
        var targetDirectory = Path.Combine(outputDirectory, GetPackageRoot(package));
        Directory.CreateDirectory(targetDirectory);

        foreach (var (path, content) in files)
        {
            File.WriteAllText(Path.Combine(targetDirectory, path), content);
        }
    }

    private static void WriteZip(
        MutantPackage package, string outputDirectory, IReadOnlyList<(string Path, string Content)> files)
    {
        Directory.CreateDirectory(outputDirectory);
        var zipPath = Path.Combine(outputDirectory, $"{GetPackageRoot(package)}.zip");

        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var (path, content) in files)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(content);
        }
    }

    private static string GetPackageRoot(MutantPackage package)
    {
        var programName = package.SourceProgram?.Name ?? package.Id;
        return BuildMutantId(programName, 1);
    }

    private static ManifestDto BuildManifest(MutantPackage package)
    {
        var program = package.SourceProgram;
        var programName = program?.Name ?? package.Id;

        return new ManifestDto
        {
            MutantId = GetPackageRoot(package),
            OriginalProgram = programName,
            BaseProgramHash = program?.SourceHash ?? string.Empty,
            Timestamp = DateTimeOffset.UtcNow,
            MutationProfile = package.Profile.Name,
            Mutations = BuildEntries(package, programName),
            SourceCopied = program is not null,
            CopybooksResolved = program?.Copybooks.Count > 0
        };
    }

    private static MutationsReportDto BuildReport(MutantPackage package)
    {
        var programName = package.SourceProgram?.Name ?? package.Id;
        var entries = BuildEntries(package, programName);

        return new MutationsReportDto
        {
            MutantId = GetPackageRoot(package),
            OriginalProgram = programName,
            GeneratedAt = DateTimeOffset.UtcNow,
            TotalMutations = entries.Count,
            Mutations = entries
        };
    }

    private static IReadOnlyList<MutationEntryDto> BuildEntries(MutantPackage package, string programName)
    {
        var entries = new List<MutationEntryDto>(package.Count);
        for (var index = 0; index < package.Mutants.Count; index++)
        {
            var mutation = package.Mutants[index];
            entries.Add(new MutationEntryDto
            {
                Id = BuildMutantId(programName, index + 1),
                Type = ToTypeName(mutation.Type),
                Line = mutation.Line,
                Original = mutation.Original,
                Mutated = mutation.Mutated,
                TestCaseCoverage = mutation.CoveringTestIds
            });
        }

        return entries;
    }

    private static string BuildMutantId(string programName, int index)
        => $"MUT-{PackageSequence:D3}-{programName}-{index:D3}";

    private static string ToTypeName(MutationType type)
        => type switch
        {
            MutationType.AndToOr or MutationType.OrToAnd
                or MutationType.AddNot or MutationType.RemoveNot => "logical_operator",
            MutationType.AddToSubtract or MutationType.SubtractToAdd
                or MutationType.MultiplyToDivide or MutationType.DivideToMultiply => "arithmetic_operator",
            MutationType.ConstantReplacement => "constant_replacement",
            MutationType.ComplexExpressionMutation => "complex_expression",
            _ => "unknown"
        };
}
