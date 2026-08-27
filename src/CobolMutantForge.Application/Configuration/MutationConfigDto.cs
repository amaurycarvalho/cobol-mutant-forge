namespace CobolMutantForge.Application.Configuration;

public sealed class MutationConfigDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public PathsDto Paths { get; set; } = new();
    public MutationProfile MutationProfile { get; set; } = MutationProfile.Medium;
    public MutationFlagsDto MutationFlags { get; set; } = new();
    public Dictionary<string, object?> Zunit { get; set; } = new();
    public Dictionary<string, object?> TestAccelerator { get; set; } = new();
    public Dictionary<string, object?> Export { get; set; } = new();
}
