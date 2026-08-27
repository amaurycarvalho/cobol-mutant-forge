using System.Text.Json;
using Xunit;

namespace CobolMutantForge.Tests.QA;

public class StrykerConfigurationTests
{
    [Fact]
    public void StrykerConfig_ExistsAndDeclaresMutatePathsReportersAndThresholds()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "stryker-config.json");
        Assert.True(File.Exists(path));

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var config = document.RootElement.GetProperty("stryker-config");

        Assert.True(config.TryGetProperty("mutate", out var mutate));
        Assert.Contains(mutate.EnumerateArray(), entry => entry.GetString()!.Contains("Domain"));
        Assert.Contains(mutate.EnumerateArray(), entry => entry.GetString()!.Contains("Application"));

        Assert.True(config.TryGetProperty("reporters", out var reporters));
        Assert.Contains(reporters.EnumerateArray(), reporter => reporter.GetString() == "html");
        Assert.Contains(reporters.EnumerateArray(), reporter => reporter.GetString() == "progress");

        var thresholds = config.GetProperty("thresholds");
        Assert.Equal(80, thresholds.GetProperty("high").GetInt32());
        Assert.Equal(60, thresholds.GetProperty("low").GetInt32());
        Assert.Equal(0, thresholds.GetProperty("break").GetInt32());
        Assert.Equal("mtp", config.GetProperty("test-runner").GetString());
        Assert.Equal("off", config.GetProperty("coverage-analysis").GetString());
    }
}
