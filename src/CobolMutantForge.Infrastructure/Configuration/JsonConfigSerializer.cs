using System.Text.Json;
using System.Text.Json.Serialization;
using CobolMutantForge.Application.Configuration;

namespace CobolMutantForge.Infrastructure.Configuration;

public sealed class JsonConfigSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public string Serialize(MutationConfigDto config)
    {
        return JsonSerializer.Serialize(config, Options);
    }

    public MutationConfigDto Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MutationConfigDto>(json, Options)
            ?? throw new JsonException("Failed to deserialize the configuration.");
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return options;
    }
}
