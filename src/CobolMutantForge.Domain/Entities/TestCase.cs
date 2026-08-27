namespace CobolMutantForge.Domain.Entities;

public sealed class TestCase
{
    public string Id { get; }
    public IReadOnlyDictionary<string, string> Inputs { get; }
    public IReadOnlyDictionary<string, string> ExpectedOutputs { get; }

    public TestCase(string id, IReadOnlyDictionary<string, string>? inputs = null, IReadOnlyDictionary<string, string>? expectedOutputs = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Inputs = inputs ?? new Dictionary<string, string>();
        ExpectedOutputs = expectedOutputs ?? new Dictionary<string, string>();
    }

    public override bool Equals(object? obj) => Equals(obj as TestCase);

    public bool Equals(TestCase? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
