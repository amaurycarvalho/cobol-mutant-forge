namespace CobolMutantForge.Domain.Interfaces;

public interface IImportPlugin
{
    string Name { get; }

    ImportResult Import(string inputPath);
}
