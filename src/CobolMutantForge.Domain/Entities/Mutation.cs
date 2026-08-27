using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Entities;

public sealed class Mutation
{
    public string Id { get; }
    public MutationType Type { get; }
    public int Line { get; }
    public string Original { get; }
    public string Mutated { get; }
    public IReadOnlyList<string> CoveringTestIds { get; }

    public Mutation(string id, MutationType type, int line, string original, string mutated, IReadOnlyList<string>? coveringTestIds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(mutated);

        if (line <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(line), "The mutation line must be a positive line number.");
        }

        Id = id;
        Type = type;
        Line = line;
        Original = original;
        Mutated = mutated;
        CoveringTestIds = coveringTestIds ?? Array.Empty<string>();
    }

    public override bool Equals(object? obj) => Equals(obj as Mutation);

    public bool Equals(Mutation? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
