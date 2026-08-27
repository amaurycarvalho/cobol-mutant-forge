using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;

namespace CobolMutantForge.Infrastructure.Plugins;

/// <summary>
/// Inert placeholder for the IBM Test Accelerator for Z integration (planned for
/// v2.0). It realizes the plugin contracts but reports that it is not yet
/// supported instead of performing an import/export.
/// </summary>
public sealed class TestAcceleratorPlugin : PluginBase, IImportPlugin, IExportPlugin
{
    public override string Name => "testaccelerator";

    public override string Version => "2.0.0";

    public ImportResult Import(string inputPath)
        => throw new NotSupportedException(
            "IBM Test Accelerator for Z is not yet supported; planned for v2.0.");

    public void Export(MutantPackage package, string outputDirectory)
        => throw new NotSupportedException(
            "IBM Test Accelerator for Z is not yet supported; planned for v2.0.");
}
