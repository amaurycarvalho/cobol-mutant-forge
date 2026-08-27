namespace CobolMutantForge.Tests.BDD.Gherkin;

public sealed record GherkinStep(string Keyword, string Text);

public sealed record GherkinScenario(string Name, IReadOnlyList<GherkinStep> Steps);

public sealed record GherkinFeature(string Name, IReadOnlyList<GherkinScenario> Scenarios);
