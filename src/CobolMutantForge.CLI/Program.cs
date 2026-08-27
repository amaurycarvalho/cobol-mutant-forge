using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using CobolMutantForge.CLI.Commands;
using CobolMutantForge.CLI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var quietOption = CreateQuietOption();

        var rootCommand = new RootCommand("CobolMutantForge - COBOL mutation testing tool")
        {
            quietOption
        };

        var services = new ServiceCollection().AddCobolMutantForge().BuildServiceProvider();
        using var loggerFactory = CreateLoggerFactory();
        var logger = loggerFactory.CreateLogger("CobolMutantForge.CLI");

        rootCommand.Add(InitCommand.Create(logger));
        rootCommand.Add(GenerateCommand.Create(services, logger));
        rootCommand.Add(ExportCommand.Create(logger));
        rootCommand.Add(PluginCommand.Create(services, logger));

        rootCommand.SetAction((Func<ParseResult, int>)(_ =>
        {
            logger.LogInformation("CobolMutantForge is ready.");
            return 0;
        }));

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync(parseResult.InvocationConfiguration, CancellationToken.None);
    }

    private static Option<bool> CreateQuietOption()
        => new("--quiet")
        {
            Description = "Suppress informational output; only errors are shown."
        };

    private static ILoggerFactory CreateLoggerFactory()
        => LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options => options.SingleLine = true);
            builder.SetMinimumLevel(LogLevel.Information);
        });
}
