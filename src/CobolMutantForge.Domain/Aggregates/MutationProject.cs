using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Domain.Aggregates;

public sealed class MutationProject
{
    private readonly List<CobolProgram> _programs = new();
    private readonly List<TestCase> _testCases = new();

    public string ProjectName { get; }
    public IReadOnlyDictionary<string, string> Paths { get; }
    public MutationProfile Profile { get; }
    public IReadOnlyList<CobolProgram> Programs => _programs;
    public IReadOnlyList<TestCase> TestCases => _testCases;

    public MutationProject(
        string projectName,
        IReadOnlyDictionary<string, string>? paths = null,
        MutationProfile? profile = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);

        ProjectName = projectName;
        Paths = paths ?? new Dictionary<string, string>();
        Profile = profile ?? MutationProfile.Medium;
    }

    public void AddProgram(CobolProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);
        _programs.Add(program);
    }

    public void AddTestCase(TestCase testCase)
    {
        ArgumentNullException.ThrowIfNull(testCase);
        _testCases.Add(testCase);
    }
}
