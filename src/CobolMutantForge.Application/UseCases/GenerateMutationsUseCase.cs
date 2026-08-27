using CobolMutantForge.Application.Services;
using CobolMutantForge.Domain.Aggregates;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;

namespace CobolMutantForge.Application.UseCases;

public sealed record MutationGenerationResult
{
    public IReadOnlyList<MutantPackage> Packages { get; init; } = Array.Empty<MutantPackage>();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Coordinates parse → strategy selection → mutation generation → validation for a
/// <see cref="MutationProject"/>, producing one <see cref="MutantPackage"/> per
/// program. Keeps the domain engine pure by handling parsing and validation here.
/// </summary>
public sealed class GenerateMutationsUseCase
{
    private readonly IMutationEngine _engine;
    private readonly ICobolParser _parser;
    private readonly ValidationService _validationService;

    public GenerateMutationsUseCase(
        IMutationEngine engine,
        ICobolParser parser,
        ValidationService validationService)
    {
        _engine = engine;
        _parser = parser;
        _validationService = validationService;
    }

    public MutationGenerationResult Execute(MutationProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var packages = new List<MutantPackage>();
        var warnings = new List<string>();

        foreach (var program in project.Programs)
        {
            var parsed = EnsureParsed(program, warnings);
            var mutations = _engine.GenerateMutations(parsed, project.Profile, project.TestCases);

            var package = new MutantPackage(parsed.Name, parsed) { Profile = project.Profile };
            foreach (var mutation in mutations)
            {
                if (_validationService.IsApplicable(parsed, mutation))
                {
                    package.AddMutant(mutation);
                }
            }

            packages.Add(package);
        }

        return new MutationGenerationResult
        {
            Packages = packages,
            Warnings = warnings
        };
    }

    private CobolProgram EnsureParsed(CobolProgram program, List<string> warnings)
    {
        if (program.Ast is not null)
        {
            return program;
        }

        var parseResult = _parser.Parse(program.SourceText);
        if (parseResult.HasErrors)
        {
            warnings.Add($"Program '{program.Name}' failed to parse; mutations were generated heuristically.");
        }

        return new CobolProgram(program.Name, program.SourceText, program.Copybooks, parseResult.Ast);
    }
}
