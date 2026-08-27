using CobolMutantForge.Domain.Entities;

namespace CobolMutantForge.Domain.Entities;

public sealed class MutantPackage
{
    private readonly List<Mutation> _mutants = new();

    public string Id { get; }
    public IReadOnlyList<Mutation> Mutants => _mutants;
    public string? Manifest { get; set; }
    public string? Report { get; set; }
    public CobolProgram? SourceProgram { get; }
    public int Count => _mutants.Count;

    public MutantPackage(string id, CobolProgram? sourceProgram = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        SourceProgram = sourceProgram;
    }

    public void AddMutant(Mutation mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        _mutants.Add(mutation);
    }

    public override bool Equals(object? obj) => Equals(obj as MutantPackage);

    public bool Equals(MutantPackage? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
