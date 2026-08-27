using System.CommandLine;
using CobolMutantForge.Application.Configuration;
using CobolMutantForge.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace CobolMutantForge.CLI.Commands;

public static class InitCommand
{
    public static Command Create(ILogger logger)
    {
        var directoryOption = new Option<string>("--directory")
        {
            Description = "Directory in which to create the configuration file.",
            DefaultValueFactory = _ => "."
        };
        var profileOption = new Option<string>("--profile")
        {
            Description = "Mutation profile: low, medium, or high.",
            DefaultValueFactory = _ => "medium"
        };
        var quietOption = new Option<bool>("--quiet")
        {
            Description = "Suppress informational output; only errors are shown."
        };

        var command = new Command("init", "Create a cobolmutantforge.json configuration file.")
        {
            directoryOption,
            profileOption,
            quietOption
        };

        command.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var quiet = parseResult.GetValue(quietOption);
            try
            {
                var directory = parseResult.GetValue(directoryOption) ?? ".";
                var config = DefaultConfigFactory.CreateDefault();
                config.MutationProfile = ParseProfile(parseResult.GetValue(profileOption) ?? "medium");

                var serializer = new JsonConfigSerializer();
                var path = Path.Combine(directory, "cobolmutantforge.json");
                Directory.CreateDirectory(directory);
                File.WriteAllText(path, serializer.Serialize(config));

                if (!quiet)
                {
                    logger.LogInformation("Created configuration at {Path}.", path);
                }

                return 0;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Init failed.");
                return 1;
            }
        }));

        return command;
    }

    private static MutationProfile ParseProfile(string name)
        => Enum.TryParse<MutationProfile>(name, ignoreCase: true, out var profile)
            ? profile
            : throw new ArgumentException($"Unknown profile '{name}'. Expected low, medium, or high.");
}
