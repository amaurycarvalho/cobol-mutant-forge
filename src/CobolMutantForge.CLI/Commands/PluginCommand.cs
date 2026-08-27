using System.CommandLine;
using CobolMutantForge.Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI.Commands;

public static class PluginCommand
{
    public static Command Create(IServiceProvider services, ILogger logger)
    {
        var quietOption = new Option<bool>("--quiet")
        {
            Description = "Suppress informational output; only errors are shown."
        };

        var listCommand = new Command("list", "List all available plugins.")
        {
            quietOption
        };

        listCommand.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var quiet = parseResult.GetValue(quietOption);
            foreach (var plugin in services.GetServices<PluginBase>())
            {
                var status = plugin.Version.StartsWith("2.", StringComparison.Ordinal)
                    ? "unavailable (planned for v2.0)"
                    : "available";
                logger.LogInformation("{Name,-16} {Status}", plugin.Name, status);
            }

            return 0;
        }));

        var pluginCommand = new Command("plugin", "Inspect available plugins.");
        pluginCommand.Add(listCommand);
        return pluginCommand;
    }
}
