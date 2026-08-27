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
    public void Deserialization_NullJson_ThrowsWithMessage()
    {
        var serializer = new JsonConfigSerializer();

        var exception = Assert.Throws<JsonException>(() => serializer.Deserialize("null"));

        Assert.Contains("Failed to deserialize the configuration.", exception.Message);
    }

    [Fact]
    public void Serialization_WritesProfileAsString()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();

        var json = serializer.Serialize(config);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("medium", document.RootElement.GetProperty("mutationProfile").GetString());
    }

    [Fact]
    public void Deserialization_RejectsNumericEnumValueZero()
    {
        var serializer = new JsonConfigSerializer();

        // allowIntegerValues is false: numeric enum values must be rejected.
        Assert.Throws<JsonException>(() => serializer.Deserialize("""{"mutationProfile":0}"""));
    }

    [Fact]
    public void Deserialization_RejectsNumericMutationProfile()
    {
        var serializer = new JsonConfigSerializer();
        const string json = """{"mutationProfile":5}""";

        Assert.Throws<JsonException>(() => serializer.Deserialize(json));
    }

    [Fact]
    public void Serialization_UsesIndentedFormatting()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();

        var json = serializer.Serialize(config);

        Assert.Contains("\n", json);
    }

    [Fact]
    public void Deserialization_IsCaseInsensitive()
    {
        var serializer = new JsonConfigSerializer();
        const string json = """{"PROJECTNAME":"case-test","MUTATIONPROFILE":"high"}""";

        var config = serializer.Deserialize(json);

        Assert.Equal("case-test", config.ProjectName);
        Assert.Equal(MutationProfile.High, config.MutationProfile);
    }

    [Fact]
    public void Deserialization_PartialJson_UsesDefaultsForMissingProperties()
    {
        var serializer = new JsonConfigSerializer();
        const string json = """{"projectName":"partial"}""";

        var config = serializer.Deserialize(json);

        Assert.Equal("partial", config.ProjectName);
        Assert.Equal(MutationProfile.Medium, config.MutationProfile);
        Assert.NotNull(config.Paths);
    }

    [Fact]
    public void Serialization_RoundTripsLowProfile()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();
        config.MutationProfile = MutationProfile.Low;
        config.MutationFlags = new MutationFlagsDto
        {
            LogicalOperators = true,
            ArithmeticOperators = false,
            ComplexExpressions = false,
            NumericConstants = false,
            StringConstants = false
        };

        var roundTripped = serializer.Deserialize(serializer.Serialize(config));

        Assert.Equal(MutationProfile.Low, roundTripped.MutationProfile);
        Assert.True(roundTripped.MutationFlags.LogicalOperators);
        Assert.False(roundTripped.MutationFlags.ArithmeticOperators);
    }

    [Fact]
    public void Serialization_RoundTripsZunitPluginSettings()
    {
        var serializer = new JsonConfigSerializer();
        var config = DefaultConfigFactory.CreateDefault();
        config.Zunit["timeout"] = 30L;
        config.Export["format"] = "zip";

        var roundTripped = serializer.Deserialize(serializer.Serialize(config));

        var timeout = Assert.IsType<JsonElement>(roundTripped.Zunit["timeout"]);
        Assert.Equal(30L, timeout.GetInt64());
        var format = Assert.IsType<JsonElement>(roundTripped.Export["format"]);
        Assert.Equal("zip", format.GetString());
    }
}
