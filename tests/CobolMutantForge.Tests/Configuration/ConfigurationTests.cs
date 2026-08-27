using CobolMutantForge.Application.Configuration;
using Xunit;

namespace CobolMutantForge.Tests.Configuration;

public class DefaultConfigFactoryTests
{
    [Fact]
    public void CreateDefault_ReturnsExpectedProjectNameAndVersion()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.Equal("CobolMutantForge", config.ProjectName);
        Assert.Equal("0.1.0", config.Version);
    }

    [Fact]
    public void CreateDefault_ReturnsExpectedPaths()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.Equal("src", config.Paths.SourceDirectory);
        Assert.Equal("tests", config.Paths.TestDataDirectory);
        Assert.Equal("output", config.Paths.OutputDirectory);
        Assert.Equal("copybooks", config.Paths.CopybookDirectory);
    }

    [Fact]
    public void CreateDefault_ReturnsMediumProfile()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.Equal(MutationProfile.Medium, config.MutationProfile);
    }

    [Fact]
    public void CreateDefault_ReturnsExpectedMutationFlags()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.True(config.MutationFlags.LogicalOperators);
        Assert.True(config.MutationFlags.ArithmeticOperators);
        Assert.False(config.MutationFlags.ComplexExpressions);
        Assert.True(config.MutationFlags.NumericConstants);
        Assert.False(config.MutationFlags.StringConstants);
    }

    [Fact]
    public void CreateDefault_ReturnsEmptyPluginAndExportDictionaries()
    {
        var config = DefaultConfigFactory.CreateDefault();

        Assert.Empty(config.Zunit);
        Assert.Empty(config.TestAccelerator);
        Assert.Empty(config.Export);
    }
}

public class PathsDtoTests
{
    [Fact]
    public void PathsDto_DefaultsAreEmpty()
    {
        var paths = new PathsDto();

        Assert.Equal(string.Empty, paths.SourceDirectory);
        Assert.Equal(string.Empty, paths.TestDataDirectory);
        Assert.Equal(string.Empty, paths.OutputDirectory);
        Assert.Equal(string.Empty, paths.CopybookDirectory);
    }

    [Fact]
    public void PathsDto_ExposesSetValues()
    {
        var paths = new PathsDto
        {
            SourceDirectory = "a",
            TestDataDirectory = "b",
            OutputDirectory = "c",
            CopybookDirectory = "d"
        };

        Assert.Equal("a", paths.SourceDirectory);
        Assert.Equal("b", paths.TestDataDirectory);
        Assert.Equal("c", paths.OutputDirectory);
        Assert.Equal("d", paths.CopybookDirectory);
    }
}

public class MutationConfigDtoTests
{
    [Fact]
    public void MutationConfigDto_DefaultsAreExpected()
    {
        var config = new MutationConfigDto();

        Assert.Equal(string.Empty, config.ProjectName);
        Assert.Equal(string.Empty, config.Version);
        Assert.NotNull(config.Paths);
        Assert.Equal(MutationProfile.Medium, config.MutationProfile);
        Assert.NotNull(config.MutationFlags);
        Assert.Empty(config.Zunit);
        Assert.Empty(config.TestAccelerator);
        Assert.Empty(config.Export);
    }

    [Fact]
    public void MutationConfigDto_ExposesSetValues()
    {
        var config = new MutationConfigDto
        {
            ProjectName = "sample",
            Version = "9.9.9",
            MutationProfile = MutationProfile.High,
            Zunit = new Dictionary<string, object?> { ["k"] = "v" }
        };

        Assert.Equal("sample", config.ProjectName);
        Assert.Equal("9.9.9", config.Version);
        Assert.Equal(MutationProfile.High, config.MutationProfile);
        Assert.Equal("v", config.Zunit["k"]);
    }
}

public class MutationFlagsDtoTests
{
    [Fact]
    public void MutationFlagsDto_DefaultsAreFalse()
    {
        var flags = new MutationFlagsDto();

        Assert.False(flags.LogicalOperators);
        Assert.False(flags.ArithmeticOperators);
        Assert.False(flags.ComplexExpressions);
        Assert.False(flags.NumericConstants);
        Assert.False(flags.StringConstants);
    }

    [Fact]
    public void MutationFlagsDto_ExposesSetValues()
    {
        var flags = new MutationFlagsDto
        {
            LogicalOperators = true,
            ArithmeticOperators = true,
            ComplexExpressions = true,
            NumericConstants = true,
            StringConstants = true
        };

        Assert.True(flags.LogicalOperators);
        Assert.True(flags.ArithmeticOperators);
        Assert.True(flags.ComplexExpressions);
        Assert.True(flags.NumericConstants);
        Assert.True(flags.StringConstants);
    }
}
