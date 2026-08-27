using CobolMutantForge.Tests.BDD.Gherkin;
using CobolMutantForge.Tests.BDD.Steps;
using Xunit;

namespace CobolMutantForge.Tests.BDD;

/// <summary>
/// Runs the <c>MutationGeneration.feature</c> scenarios on the Microsoft Testing
/// Platform (the project's MTP v1 runner). Each scenario is executed as its own
/// xUnit test via a <see cref="StepRunner"/> over the registered step definitions.
/// </summary>
public class MutationGenerationFeatureTests
{
    private static readonly string FeaturePath =
        Path.Combine(AppContext.BaseDirectory, "BDD", "Features", "MutationGeneration.feature");

    public static IEnumerable<object[]> Scenarios()
    {
        var feature = GherkinParser.Parse(File.ReadAllText(FeaturePath));
        return feature.Scenarios.Select(scenario => new object[] { scenario.Name });
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void Scenario_Passes(string scenarioName)
    {
        var feature = GherkinParser.Parse(File.ReadAllText(FeaturePath));
        var scenario = feature.Scenarios.Single(item => item.Name == scenarioName);

        var runner = new StepRunner();
        runner.AddSteps(new MutationGenerationSteps());

        foreach (var step in scenario.Steps)
        {
            runner.ExecuteStep(step.Keyword, step.Text);
        }
    }
}
