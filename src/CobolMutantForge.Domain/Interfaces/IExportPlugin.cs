using CobolMutantForge.Domain.Entities;

namespace CobolMutantForge.Domain.Interfaces;

public interface IExportPlugin
{
    string Name { get; }

    void Export(MutantPackage package, string outputDirectory);
}
