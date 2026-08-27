namespace CobolMutantForge.Tests.BDD.Gherkin;

/// <summary>
/// Minimal Gherkin parser for the MTP-hosted BDD scenarios: reads <c>Feature:</c> and
/// <c>Scenario:</c> headers plus <c>Given</c>/<c>When</c>/<c>Then</c>/<c>And</c>/
/// <c>But</c> steps, ignoring comments and the free-form feature narrative.
/// </summary>
internal static class GherkinParser
{
    private static readonly string[] StepKeywords = { "Given", "When", "Then", "And", "But" };

    public static GherkinFeature Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var featureName = string.Empty;
        var scenarios = new List<GherkinScenario>();
        GherkinScenarioBuilder? current = null;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("Feature:", StringComparison.Ordinal))
            {
                featureName = line["Feature:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("Scenario:", StringComparison.Ordinal))
            {
                if (current is not null)
                {
                    scenarios.Add(current.Build());
                }

                current = new GherkinScenarioBuilder(line["Scenario:".Length..].Trim());
                continue;
            }

            if (TryParseStep(line, out var step))
            {
                current?.AddStep(step);
            }
        }

        if (current is not null)
        {
            scenarios.Add(current.Build());
        }

        return new GherkinFeature(featureName, scenarios);
    }

    private static bool TryParseStep(string line, out GherkinStep step)
    {
        step = default!;
        foreach (var keyword in StepKeywords)
        {
            if (line.StartsWith(keyword + " ", StringComparison.Ordinal))
            {
                step = new GherkinStep(keyword, line[(keyword.Length + 1)..]);
                return true;
            }
        }

        return false;
    }

    private sealed class GherkinScenarioBuilder
    {
        private readonly List<GherkinStep> _steps = new();

        public GherkinScenarioBuilder(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void AddStep(GherkinStep step) => _steps.Add(step);

        public GherkinScenario Build() => new(Name, _steps);
    }
}
