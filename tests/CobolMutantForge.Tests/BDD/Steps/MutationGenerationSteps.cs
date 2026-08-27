using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.BDD.Steps;

/// <summary>
/// Step definitions backing the <c>MutationGeneration.feature</c> scenarios. The
/// <c>Then</c> step asserts the produced mutant; it is deliberately kept outside any
/// loop so the xUnit1051 guidance holds.
/// </summary>
internal sealed class MutationGenerationSteps
{
    private string? _source;
    private IReadOnlyList<Mutation> _mutations = Array.Empty<Mutation>();

    [Given("a program containing \"(.*)\"")]
    public void GivenAProgramContaining(string source)
    {
        _source = source;
    }

    [When("I generate mutations under the (low|medium|high) profile")]
    public void WhenGenerateMutations(string profileName)
    {
        var parseResult = new TypeCobolParserAdapter().Parse(_source!);
        var program = new CobolProgram("SAMPLE", _source!, null, parseResult.Ast);
        var profile = CobolMutantForge.Domain.ValueObjects.MutationProfile.FromName(profileName);
        _mutations = new MutationEngine().GenerateMutations(program, profile);
    }

    [Then("a mutant replacing \"(.*)\" with \"(.*)\" is produced")]
    public void ThenMutantIsProduced(string original, string mutated)
    {
        Assert.Contains(_mutations, mutation => mutation.Original == original && mutation.Mutated == mutated);
    }
}
