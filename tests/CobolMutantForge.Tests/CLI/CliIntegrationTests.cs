using System.Text.RegularExpressions;
using CobolMutantForge.Application.Configuration;
using CobolMutantForge.CLI;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Infrastructure.Configuration;
using CobolMutantForge.Infrastructure.Exporters;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.CLI;

[CollectionDefinition("CLI Integration", DisableParallelization = true)]
public class CliIntegrationCollectionDefinition
{
}

[Collection("CLI Integration")]
public class CliIntegrationTests
{
    private const string SampleSource =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. PAYMENT.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM PAYMENT.\r\n";

    [Fact]
    public async Task Version_PrintsToolVersion()
    {
        var (exitCode, output) = await RunCli("--version");

        Assert.Equal(0, exitCode);
        Assert.Matches(@"\d+\.\d+\.\d+", output);
    }

    [Fact]
    public async Task Init_CreatesConfigurationFile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, output) = await RunCli("init", "--directory", directory, "--profile", "high");

            Assert.Equal(0, exitCode);
            Assert.Contains("Created configuration at", output);
            Assert.Contains(Path.Combine(directory, "cobolmutantforge.json"), output);
            var configPath = Path.Combine(directory, "cobolmutantforge.json");
            Assert.True(File.Exists(configPath));
            var config = new JsonConfigSerializer().Deserialize(File.ReadAllText(configPath));
            Assert.Equal(MutationProfile.High, config.MutationProfile);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Init_DefaultProfile_WritesMediumProfile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, _) = await RunCli("init", "--directory", directory);

            Assert.Equal(0, exitCode);
            var config = new JsonConfigSerializer().Deserialize(
                File.ReadAllText(Path.Combine(directory, "cobolmutantforge.json")));
            Assert.Equal(MutationProfile.Medium, config.MutationProfile);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Init_LowProfile_WritesLowProfile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, _) = await RunCli("init", "--directory", directory, "--profile", "low");

            Assert.Equal(0, exitCode);
            var config = new JsonConfigSerializer().Deserialize(
                File.ReadAllText(Path.Combine(directory, "cobolmutantforge.json")));
            Assert.Equal(MutationProfile.Low, config.MutationProfile);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Init_Quiet_SuppressesInformationalOutput()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, output) = await RunCli("init", "--directory", directory, "--quiet");

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(Path.Combine(directory, "cobolmutantforge.json")));
            Assert.DoesNotContain("Created configuration", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Init_InvalidProfile_Fails()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, output) = await RunCli("init", "--directory", directory, "--profile", "extreme");

            Assert.NotEqual(0, exitCode);
            Assert.False(File.Exists(Path.Combine(directory, "cobolmutantforge.json")));
            Assert.Contains("Init failed.", output, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Unknown profile", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Init_CreatesNestedDirectory()
    {
        var directory = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(directory, "deep", "nested");

            var (exitCode, _) = await RunCli("init", "--directory", nested);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(Path.Combine(nested, "cobolmutantforge.json")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task PluginList_ListsZunitAndTestAccelerator()
    {
        var (exitCode, output) = await RunCli("plugin", "list");

        Assert.Equal(0, exitCode);
        Assert.Contains("zunit", output);
        Assert.Contains("testaccelerator", output);
    }

    [Fact]
    public async Task PluginList_ReportsAvailabilityStatuses()
    {
        var (exitCode, output) = await RunCli("plugin", "list");

        Assert.Equal(0, exitCode);
        Assert.Contains("available", output);
        Assert.Contains("unavailable (planned for v2.0)", output);
    }

    [Fact]
    public async Task PluginList_Quiet_StillListsPlugins()
    {
        var (exitCode, output) = await RunCli("plugin", "list", "--quiet");

        Assert.Equal(0, exitCode);
        Assert.Contains("zunit", output);
        Assert.Contains("testaccelerator", output);
    }

    [Fact]
    public async Task Generate_ProducesMutantPackagesIntoOutput()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(directory, "src"));
            File.WriteAllText(Path.Combine(directory, "src", "PAYMENT.cbl"), SampleSource);
            var configPath = WriteConfig(directory);

            var (exitCode, output) = await RunCli("generate", "--config", configPath);

            Assert.Equal(0, exitCode);
            Assert.Contains("Generated 1 mutant packages in", output);
            var outputDirectory = Path.Combine(directory, "output");
            Assert.Contains(outputDirectory, output);
            var packageDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            Assert.True(File.Exists(Path.Combine(packageDirectory, "manifest.json")));
            Assert.True(File.Exists(Path.Combine(packageDirectory, "mutations-report.json")));
            Assert.Contains(Directory.EnumerateFiles(packageDirectory, "*.cbl"), path => path.Contains("PAYMENT"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Generate_MissingConfig_Fails()
    {
        var directory = CreateTempDirectory();
        try
        {
            var (exitCode, output) = await RunCli("generate", "--config", Path.Combine(directory, "missing.json"));

            Assert.NotEqual(0, exitCode);
            Assert.Contains("Generate failed.", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Generate_Quiet_SuppressesInformationalOutput()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(directory, "src"));
            File.WriteAllText(Path.Combine(directory, "src", "PAYMENT.cbl"), SampleSource);
            var configPath = WriteConfig(directory);

            var (exitCode, output) = await RunCli("generate", "--config", configPath, "--quiet");

            Assert.Equal(0, exitCode);
            Assert.DoesNotContain("Generated", output, StringComparison.OrdinalIgnoreCase);
            Assert.Single(Directory.EnumerateDirectories(Path.Combine(directory, "output")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Generate_OutputOptionOverridesConfig()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(directory, "src"));
            File.WriteAllText(Path.Combine(directory, "src", "PAYMENT.cbl"), SampleSource);
            var configPath = WriteConfig(directory);
            var customOutput = Path.Combine(directory, "custom-output");

            var (exitCode, _) = await RunCli("generate", "--config", configPath, "--output", customOutput);

            Assert.Equal(0, exitCode);
            Assert.True(Directory.Exists(customOutput));
            Assert.False(Directory.Exists(Path.Combine(directory, "output")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Export_Folder_RepackagesSampleMutantSet()
    {
        var directory = CreateTempDirectory();
        try
        {
            var sourceDirectory = Path.Combine(directory, "generated");
            var outputDirectory = Path.Combine(directory, "packages");
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            exporter.Export(CreateSamplePackage(), sourceDirectory);

            var (exitCode, output) = await RunCli(
                "export", "--source", sourceDirectory, "--output", outputDirectory, "--format", "folder");

            Assert.Equal(0, exitCode);
            Assert.Contains("Exported 1 mutant packages to", output);
            Assert.Contains(outputDirectory, output);
            var packageDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            Assert.True(File.Exists(Path.Combine(packageDirectory, "manifest.json")));
            Assert.True(File.Exists(Path.Combine(packageDirectory, "mutations-report.json")));
            Assert.Contains(Directory.EnumerateFiles(packageDirectory, "*.cbl"), path => path.EndsWith(".cbl"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Export_InvalidFormat_Fails()
    {
        var directory = CreateTempDirectory();
        try
        {
            var sourceDirectory = Path.Combine(directory, "generated");
            Directory.CreateDirectory(sourceDirectory);
            new MutantPackageExporter(ExportFormat.Folder).Export(CreateSamplePackage(), sourceDirectory);

            var (exitCode, output) = await RunCli(
                "export", "--source", sourceDirectory, "--output", Path.Combine(directory, "out"), "--format", "tar");

            Assert.NotEqual(0, exitCode);
            Assert.Contains("Export failed.", output, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Unknown format", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateHelp_ShowsDescriptionAndOptions()
    {
        var (exitCode, output) = await RunCli("generate", "--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Generate mutants based on the project configuration", output);
        Assert.Contains("Path to the configuration file.", output);
        Assert.Contains("Plugin to use: zunit or testaccelerator.", output);
        Assert.Contains("Output directory (overrides configuration).", output);
        Assert.Contains("Suppress informational output; only errors are shown.", output);
    }

    [Fact]
    public async Task ExportHelp_ShowsDescriptionAndOptions()
    {
        var (exitCode, output) = await RunCli("export", "--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Package generated mutants for manual CICS import", output);
        Assert.Contains("Directory containing generated mutants.", output);
        Assert.Contains("Output directory for the package.", output);
        Assert.Contains("Package format: zip or folder.", output);
        Assert.Contains("Suppress informational output; only errors are shown.", output);
    }

    [Fact]
    public async Task InitHelp_ShowsDescriptionAndOptions()
    {
        var (exitCode, output) = await RunCli("init", "--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Create a cobolmutantforge.json configuration file", output);
        Assert.Contains("Mutation profile: low, medium, or high.", output);
        Assert.Contains("Directory in which to create the configuration file.", output);
        Assert.Contains("Suppress informational output; only errors are shown.", output);
    }

    [Fact]
    public async Task PluginHelp_ShowsDescription()
    {
        var (exitCode, output) = await RunCli("plugin", "--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Inspect available plugins", output);
        Assert.Contains("list", output);
        Assert.Contains("List all available plugins.", output);
    }

    [Fact]
    public async Task Export_DefaultFormat_IsFolder()
    {
        var directory = CreateTempDirectory();
        try
        {
            var sourceDirectory = Path.Combine(directory, "generated");
            new MutantPackageExporter(ExportFormat.Folder).Export(CreateSamplePackage(), sourceDirectory);

            var (exitCode, _) = await RunCli(
                "export", "--source", sourceDirectory, "--output", Path.Combine(directory, "out"));

            Assert.Equal(0, exitCode);
            Assert.Single(Directory.EnumerateDirectories(Path.Combine(directory, "out")));
            Assert.Empty(Directory.EnumerateFiles(Path.Combine(directory, "out"), "*.zip"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Export_Zip_ProducesArchiveInOutput()
    {
        var directory = CreateTempDirectory();
        try
        {
            var sourceDirectory = Path.Combine(directory, "generated");
            var outputDirectory = Path.Combine(directory, "packages");
            new MutantPackageExporter(ExportFormat.Folder).Export(CreateSamplePackage(), sourceDirectory);

            var (exitCode, _) = await RunCli(
                "export", "--source", sourceDirectory, "--output", outputDirectory, "--format", "zip");

            Assert.Equal(0, exitCode);
            Assert.Single(Directory.EnumerateFiles(outputDirectory, "*.zip"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<(int ExitCode, string Output)> RunCli(params string[] args)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var exitCode = await Program.Main(args);
            return (exitCode, writer.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static string WriteConfig(string directory)
    {
        var config = new MutationConfigDto
        {
            ProjectName = "sample",
            Paths = new PathsDto
            {
                SourceDirectory = Path.Combine(directory, "src"),
                TestDataDirectory = Path.Combine(directory, "tests"),
                OutputDirectory = Path.Combine(directory, "output"),
                CopybookDirectory = Path.Combine(directory, "copybooks")
            },
            MutationProfile = MutationProfile.Medium,
            MutationFlags = new MutationFlagsDto
            {
                LogicalOperators = true,
                ArithmeticOperators = true
            }
        };

        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, new JsonConfigSerializer().Serialize(config));
        return path;
    }

    private static MutantPackage CreateSamplePackage()
    {
        var parser = new TypeCobolParserAdapter();
        var parseResult = parser.Parse(SampleSource);
        var program = new CobolProgram("PAYMENT", SampleSource, new[] { "CUSTOMER" }, parseResult.Ast);

        var package = new MutantPackage("PAYMENT", program)
        {
            Profile = CobolMutantForge.Domain.ValueObjects.MutationProfile.Medium
        };
        package.AddMutant(new Mutation("MUT-PAYMENT-001", CobolMutantForge.Domain.ValueObjects.MutationType.AndToOr, 4, "AND", "OR"));
        return package;
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "cmf-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
