using System.CommandLine;
using CobolMutantForge.Application.UseCases;
using CobolMutantForge.Infrastructure.Exporters;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI.Commands;

public static class ExportCommand
{
    public static Command Create(ILogger logger)
    {
        var sourceOption = new Option<string>("--source")
        {
            Description = "Directory containing generated mutants.",
            Required = true
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "Output directory for the package.",
            Required = true
        };
        var formatOption = new Option<string>("--format")
        {
            Description = "Package format: zip or folder.",
            DefaultValueFactory = _ => "folder"
        };
        var quietOption = new Option<bool>("--quiet")
        {
            Description = "Suppress informational output; only errors are shown."
        };

        var command = new Command("export", "Package generated mutants for manual CICS import.")
        {
            sourceOption,
            outputOption,
            formatOption,
            quietOption
        };

        command.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var quiet = parseResult.GetValue(quietOption);
            try
            {
                var source = parseResult.GetValue(sourceOption)!;
                var output = parseResult.GetValue(outputOption)!;
                var format = ParseFormat(parseResult.GetValue(formatOption) ?? "folder");

                var reader = new PackageManifestReader();
                var packages = reader.ReadAll(source);

                var exporter = new MutantPackageExporter(format);
                var useCase = new ExportMutantsUseCase(exporter);
                useCase.Execute(packages, output);

                if (!quiet)
                {
                    logger.LogInformation("Exported {Count} mutant packages to {Output}.", packages.Count, output);
                }

                return 0;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Export failed.");
                return 1;
            }
        }));

        return command;
    }

    private static ExportFormat ParseFormat(string name)
        => name.ToLowerInvariant() switch
        {
            "zip" => ExportFormat.Zip,
            "folder" => ExportFormat.Folder,
            _ => throw new ArgumentException($"Unknown format '{name}'. Expected zip or folder.")
        };
}
