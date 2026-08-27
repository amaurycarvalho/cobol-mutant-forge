using System.CommandLine;
using CobolMutantForge.Application.Configuration;
using CobolMutantForge.Application.UseCases;
using CobolMutantForge.Domain.Aggregates;
using CobolMutantForge.Infrastructure.Configuration;
using CobolMutantForge.Infrastructure.Exporters;
using CobolMutantForge.Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI.Commands;

public static class GenerateCommand
{
    public static Command Create(IServiceProvider services, ILogger logger)
    {
        var configOption = new Option<string>("--config")
        {
            Description = "Path to the configuration file.",
            DefaultValueFactory = _ => "cobolmutantforge.json"
        };
        var pluginOption = new Option<string>("--plugin")
        {
            Description = "Plugin to use: zunit or testaccelerator.",
            DefaultValueFactory = _ => "zunit"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "Output directory (overrides configuration)."
        };
        var quietOption = new Option<bool>("--quiet")
        {
            Description = "Suppress informational output; only errors are shown."
        };

        var command = new Command("generate", "Generate mutants based on the project configuration.")
        {
            configOption,
            pluginOption,
            outputOption,
            quietOption
        };

        command.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var quiet = parseResult.GetValue(quietOption);
            try
            {
                var configPath = parseResult.GetValue(configOption) ?? "cobolmutantforge.json";
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("Configuration file not found.", configPath);
                }

                var serializer = services.GetRequiredService<JsonConfigSerializer>();
                var config = serializer.Deserialize(File.ReadAllText(configPath));

                var useCase = services.GetRequiredService<GenerateMutationsUseCase>();
                var project = BuildProject(config);
                var result = useCase.Execute(project);

                var outputDirectory = parseResult.GetValue(outputOption) ?? config.Paths.OutputDirectory;
                var exporter = new MutantPackageExporter(ExportFormat.Folder);
                foreach (var package in result.Packages)
                {
                    exporter.Export(package, outputDirectory);
                }

                if (!quiet)
                {
                    logger.LogInformation(
                        "Generated {Count} mutant packages in {Output}.",
                        result.Packages.Count,
                        outputDirectory);
                }

                return 0;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Generate failed.");
                return 1;
            }
        }));

        return command;
    }

    private static MutationProject BuildProject(MutationConfigDto config)
    {
        var profile = CobolMutantForge.Domain.ValueObjects.MutationProfile
            .FromName(config.MutationProfile.ToString().ToLowerInvariant());

        var project = new MutationProject(config.ProjectName, BuildPaths(config.Paths), profile);
        var plugin = new ZUnitPlugin(config.Paths.CopybookDirectory);
        var import = plugin.Import(config.Paths.SourceDirectory);
        foreach (var program in import.Programs)
        {
            project.AddProgram(program);
        }

        foreach (var testCase in import.TestCases)
        {
            project.AddTestCase(testCase);
        }

        return project;
    }

    private static IReadOnlyDictionary<string, string> BuildPaths(PathsDto paths)
        => new Dictionary<string, string>
        {
            ["sourceDirectory"] = paths.SourceDirectory,
            ["testDataDirectory"] = paths.TestDataDirectory,
            ["outputDirectory"] = paths.OutputDirectory,
            ["copybookDirectory"] = paths.CopybookDirectory
        };
}
