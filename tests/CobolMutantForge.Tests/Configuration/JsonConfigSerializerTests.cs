using System.Text.Json;
using CobolMutantForge.Application.Configuration;
using CobolMutantForge.Infrastructure.Configuration;
using Xunit;

namespace CobolMutantForge.Tests.Configuration;

public class JsonConfigSerializerTests
{
    [Fact]
    public void DefaultConfiguration_SerializesAllTopLevelSections()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();

        var json = serializer.Serialize(config);
        using var document = JsonDocument.Parse(json);

        Assert.True(document.RootElement.TryGetProperty("projectName", out _));
        Assert.True(document.RootElement.TryGetProperty("version", out _));
        Assert.True(document.RootElement.TryGetProperty("paths", out _));
        Assert.True(document.RootElement.TryGetProperty("mutationProfile", out _));
        Assert.True(document.RootElement.TryGetProperty("mutationFlags", out _));
        Assert.True(document.RootElement.TryGetProperty("zunit", out _));
        Assert.True(document.RootElement.TryGetProperty("testAccelerator", out _));
        Assert.True(document.RootElement.TryGetProperty("export", out _));
    }

    [Fact]
    public void DefaultConfiguration_RoundTripsThroughDeserialization()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();

        var json = serializer.Serialize(config);
        var roundTripped = serializer.Deserialize(json);

        Assert.Equal(config.ProjectName, roundTripped.ProjectName);
        Assert.Equal(config.Version, roundTripped.Version);
        Assert.Equal(config.Paths.SourceDirectory, roundTripped.Paths.SourceDirectory);
        Assert.Equal(config.Paths.TestDataDirectory, roundTripped.Paths.TestDataDirectory);
        Assert.Equal(config.Paths.OutputDirectory, roundTripped.Paths.OutputDirectory);
        Assert.Equal(config.Paths.CopybookDirectory, roundTripped.Paths.CopybookDirectory);
        Assert.Equal(config.MutationProfile, roundTripped.MutationProfile);
        Assert.Equal(config.MutationFlags.LogicalOperators, roundTripped.MutationFlags.LogicalOperators);
        Assert.Equal(config.MutationFlags.ArithmeticOperators, roundTripped.MutationFlags.ArithmeticOperators);
        Assert.Equal(config.MutationFlags.ComplexExpressions, roundTripped.MutationFlags.ComplexExpressions);
        Assert.Equal(config.MutationFlags.NumericConstants, roundTripped.MutationFlags.NumericConstants);
        Assert.Equal(config.MutationFlags.StringConstants, roundTripped.MutationFlags.StringConstants);
    }

    [Fact]
    public void DefaultConfiguration_UsesMediumMutationProfile()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.Equal(MutationProfile.Medium, config.MutationProfile);
    }

    [Fact]
    public void Deserialization_RejectsUnknownMutationProfile()
    {
        var serializer = new JsonConfigSerializer();
        const string json = """{"mutationProfile":"extreme"}""";

        Assert.Throws<JsonException>(() => serializer.Deserialize(json));
    }

    [Fact]
    public void Deserialization_RejectsNumericMutationProfile()
    {
        var serializer = new JsonConfigSerializer();
        const string json = """{"mutationProfile":5}""";

        Assert.Throws<JsonException>(() => serializer.Deserialize(json));
    }
}
