using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var quietOption = new Option<bool>(
            "--quiet",
            "Suppress informational output; only errors are shown.");

        var rootCommand = new RootCommand("CobolMutantForge - COBOL mutation testing tool")
        {
            quietOption
        };

        rootCommand.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var quiet = parseResult.GetValue(quietOption);
            using var loggerFactory = CreateLoggerFactory(quiet);
            var logger = loggerFactory.CreateLogger("CobolMutantForge.CLI");
            logger.LogInformation("CobolMutantForge is ready.");
            return 0;
        }));

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync(parseResult.InvocationConfiguration, CancellationToken.None);
    }

    private static ILoggerFactory CreateLoggerFactory(bool quiet)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
            });
            builder.SetMinimumLevel(quiet ? LogLevel.Error : LogLevel.Information);
        });
    }
}
