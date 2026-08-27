using System.Text.Json;
using CobolMutantForge.Infrastructure.Plugins;

namespace CobolMutantForge.Infrastructure.Serialization;

public sealed record ZUnitConfigParseResult
{
    public ZUnitConfig Config { get; init; } = new();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    public bool IsValid { get; init; } = true;
}

/// <summary>
/// Tolerant parser for ZUnit Test Runner configuration (<c>.bzucfg</c>) files.
///
/// The format is undocumented (PRD risk), so the parser accepts either a JSON
/// object or simple <c>key = value</c> lines with optional <c>[section]</c>
/// headers. Malformed content records a warning rather than aborting the import.
/// </summary>
public sealed class ZUnitConfigParser
{
    public ZUnitConfigParseResult Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (TryParseAsJson(content, out var jsonConfig))
        {
            return new ZUnitConfigParseResult { Config = jsonConfig };
        }

        if (content.TrimStart().StartsWith('{'))
        {
            return new ZUnitConfigParseResult
            {
                IsValid = false,
                Warnings = new[] { "Malformed JSON in .bzucfg configuration." }
            };
        }

        return ParseKeyValue(content);
    }

    private static bool TryParseAsJson(string content, out ZUnitConfig config)
    {
        config = new ZUnitConfig();

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var root = document.RootElement;
            config.Name = ReadStringProperty(root, "name", "configName");
            config.TestContext = ReadStringProperty(root, "testContext", "context", "environment");

            var parameters = new Dictionary<string, string>();
            foreach (var property in root.EnumerateObject())
            {
                if (IsMetadataProperty(property.Name))
                {
                    continue;
                }

                if (property.Value.ValueKind is JsonValueKind.String
                    or JsonValueKind.Number
                    or JsonValueKind.True
                    or JsonValueKind.False)
                {
                    parameters[property.Name] = property.Value.ToString();
                }
            }

            config.Parameters = parameters;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? ReadStringProperty(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static bool IsMetadataProperty(string name)
        => name.Equals("name", StringComparison.OrdinalIgnoreCase)
            || name.Equals("configName", StringComparison.OrdinalIgnoreCase)
            || name.Equals("testContext", StringComparison.OrdinalIgnoreCase)
            || name.Equals("context", StringComparison.OrdinalIgnoreCase)
            || name.Equals("environment", StringComparison.OrdinalIgnoreCase);

    private static ZUnitConfigParseResult ParseKeyValue(string content)
    {
        var config = new ZUnitConfig();
        var parameters = new Dictionary<string, string>();
        var warnings = new List<string>();

        foreach (var rawLine in content.Split('\n'))
        {
            ApplyLine(rawLine, config, parameters, warnings);
        }

        config.Parameters = parameters;
        return new ZUnitConfigParseResult
        {
            Config = config,
            Warnings = warnings
        };
    }

    private static void ApplyLine(string rawLine, ZUnitConfig config, Dictionary<string, string> parameters, List<string> warnings)
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';'))
        {
            return;
        }

        if (line.StartsWith('[') && line.EndsWith(']'))
        {
            config.Name ??= line[1..^1].Trim();
            return;
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            warnings.Add($"Ignored unrecognized .bzucfg line: {line}");
            return;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        if (IsContextKey(key))
        {
            config.TestContext = value;
            return;
        }

        parameters[key] = value;
    }

    private static bool IsContextKey(string key)
        => key.Equals("testContext", StringComparison.OrdinalIgnoreCase)
            || key.Equals("context", StringComparison.OrdinalIgnoreCase)
            || key.Equals("environment", StringComparison.OrdinalIgnoreCase);
}
