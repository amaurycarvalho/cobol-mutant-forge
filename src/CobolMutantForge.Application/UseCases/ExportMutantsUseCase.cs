using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;

namespace CobolMutantForge.Application.UseCases;

/// <summary>
/// Exports a set of <see cref="MutantPackage"/> instances through the configured
/// <see cref="IExportPlugin"/> into the given output directory. The output format
/// (folder or zip) is chosen by the exporter implementation provided to the use case.
/// </summary>
public sealed class ExportMutantsUseCase
{
    private readonly IExportPlugin _exporter;

    public ExportMutantsUseCase(IExportPlugin exporter)
    {
        _exporter = exporter;
    }

    public void Execute(IReadOnlyList<MutantPackage> packages, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        foreach (var package in packages)
        {
            _exporter.Export(package, outputDirectory);
        }
    }
}
