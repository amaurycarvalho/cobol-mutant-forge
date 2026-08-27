namespace CobolMutantForge.Application.Configuration;

public sealed class PathsDto
{
    public string SourceDirectory { get; set; } = string.Empty;
    public string TestDataDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string CopybookDirectory { get; set; } = string.Empty;
}
