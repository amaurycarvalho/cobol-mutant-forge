using System.Text.RegularExpressions;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Infrastructure.Parsers;
using CobolMutantForge.Infrastructure.Serialization;

namespace CobolMutantForge.Infrastructure.Plugins;

/// <summary>
/// Importer for objects exported by IBM ZUnit from CICS: <c>.xml</c> test data,
/// <c>.bzucfg</c> configuration, <c>.cbl</c> source, and COPYBOOKS.
///
/// The import is tolerant by design (PRD risk: ZUnit's formats are undocumented):
/// malformed or missing artifacts are recorded as warnings and the import returns
/// whatever partial result could be assembled instead of hard-failing.
/// </summary>
public sealed class ZUnitPlugin : PluginBase, IImportPlugin
{
    private readonly ICobolParser _cobolParser = new TypeCobolParserAdapter();
    private readonly string? _copybookDirectory;

    public ZUnitPlugin(string? copybookDirectory = null)
    {
        _copybookDirectory = copybookDirectory;
    }

    public override string Name => "zunit";

    public override string Version => "1.0.0";

    ImportResult IImportPlugin.Import(string inputPath) => Import(inputPath);

    public ZUnitImportResult Import(string inputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (!Directory.Exists(inputPath))
        {
            return new ZUnitImportResult
            {
                IsValid = false,
                Warnings = new[] { $"ZUnit export directory not found: {inputPath}" }
            };
        }

        var state = new ImportState();
        var testCases = LoadTestCases(inputPath, state);
        var config = LoadConfig(inputPath, state);
        var (programs, copybooks) = LoadPrograms(inputPath, state);

        return new ZUnitImportResult
        {
            Programs = programs,
            TestCases = testCases,
            Config = config,
            Copybooks = copybooks,
            Warnings = state.Warnings,
            IsValid = !state.HasMalformedArtifact
        };
    }

    private IReadOnlyList<TestCase> LoadTestCases(string inputPath, ImportState state)
    {
        var parser = new ZUnitXmlParser();
        var testCases = new List<TestCase>();

        foreach (var xmlFile in Directory.EnumerateFiles(inputPath, "*.xml"))
        {
            var content = ReadFile(xmlFile, state);
            if (content is null)
            {
                continue;
            }

            var result = parser.Parse(content);
            state.Warnings.AddRange(result.Warnings);
            if (!result.IsValid)
            {
                state.HasMalformedArtifact = true;
            }

            testCases.AddRange(result.TestCases);
        }

        return testCases;
    }

    private ZUnitConfig LoadConfig(string inputPath, ImportState state)
    {
        var parser = new ZUnitConfigParser();
        var configFile = Directory.EnumerateFiles(inputPath, "*.bzucfg").FirstOrDefault();
        if (configFile is null)
        {
            return new ZUnitConfig();
        }

        var content = ReadFile(configFile, state);
        if (content is null)
        {
            return new ZUnitConfig();
        }

        var result = parser.Parse(content);
        state.Warnings.AddRange(result.Warnings);
        if (!result.IsValid)
        {
            state.HasMalformedArtifact = true;
        }

        return result.Config;
    }

    private (IReadOnlyList<CobolProgram> Programs, IReadOnlyList<string> Copybooks) LoadPrograms(
        string inputPath, ImportState state)
    {
        var programs = new List<CobolProgram>();
        var copybooks = new List<string>();

        foreach (var cblFile in Directory.EnumerateFiles(inputPath, "*.cbl"))
        {
            var source = ReadFile(cblFile, state);
            if (source is null)
            {
                continue;
            }

            var parseResult = _cobolParser.Parse(source);
            var name = ExtractProgramName(source) ?? Path.GetFileNameWithoutExtension(cblFile);
            var references = ExtractCopybookReferences(source);
            var resolved = ResolveCopybooks(references, inputPath, state);
            copybooks.AddRange(resolved);

            programs.Add(new CobolProgram(name, source, resolved, parseResult.Ast));
        }

        return (programs, DiscoverCopybooks(inputPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private string? ReadFile(string path, ImportState state)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            state.Warnings.Add($"Could not read file '{path}': {exception.Message}");
            state.HasMalformedArtifact = true;
            return null;
        }
    }

    private static string? ExtractProgramName(string source)
    {
        var match = Regex.Match(
            source,
            @"^\s*PROGRAM-ID\.\s+([A-Za-z0-9-]+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static IReadOnlyList<string> ExtractCopybookReferences(string source)
    {
        var references = new List<string>();
        foreach (Match match in Regex.Matches(
            source,
            @"^\s*COPY\s+([A-Za-z0-9-]+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline))
        {
            references.Add(match.Groups[1].Value);
        }

        return references;
    }

    private IReadOnlyList<string> ResolveCopybooks(
        IReadOnlyList<string> references, string inputPath, ImportState state)
    {
        var resolved = new List<string>();
        foreach (var reference in references)
        {
            var found = EnumerateCopybookDirectories(inputPath)
                .Where(Directory.Exists)
                .Select(directory => FindCopybook(directory, reference))
                .FirstOrDefault(path => path is not null);

            if (found is null)
            {
                state.Warnings.Add($"Copybook '{reference}' was referenced but not found.");
                continue;
            }

            resolved.Add(reference);
        }

        return resolved;
    }

    private IEnumerable<string> EnumerateCopybookDirectories(string inputPath)
    {
        if (!string.IsNullOrWhiteSpace(_copybookDirectory))
        {
            yield return _copybookDirectory;
        }

        yield return inputPath;
        yield return Path.Combine(inputPath, "copybooks");
    }

    private static string? FindCopybook(string directory, string reference)
    {
        foreach (var pattern in new[] { "*.cpy", "*.cob" })
        {
            foreach (var file in Directory.EnumerateFiles(directory, pattern))
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(reference, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
        }

        return null;
    }

    private IEnumerable<string> DiscoverCopybooks(string inputPath)
    {
        foreach (var directory in EnumerateCopybookDirectories(inputPath))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var pattern in new[] { "*.cpy", "*.cob" })
            {
                foreach (var file in Directory.EnumerateFiles(directory, pattern))
                {
                    var name = Path.GetFileName(file);
                    if (name is not null)
                    {
                        yield return name;
                    }
                }
            }
        }
    }

    private sealed class ImportState
    {
        public List<string> Warnings { get; } = new();

        public bool HasMalformedArtifact { get; set; }
    }
}
