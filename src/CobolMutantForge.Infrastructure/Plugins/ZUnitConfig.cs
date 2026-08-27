namespace CobolMutantForge.Infrastructure.Plugins;

/// <summary>
/// Parsed representation of a ZUnit Test Runner configuration (<c>.bzucfg</c>),
/// identifying the test parameters and context declared by the export.
/// </summary>
public sealed class ZUnitConfig
{
    public string? Name { get; set; }

    public string? TestContext { get; set; }

    public IReadOnlyDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
}
