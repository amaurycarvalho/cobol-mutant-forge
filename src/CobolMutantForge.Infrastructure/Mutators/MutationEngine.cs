using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;

namespace CobolMutantForge.Infrastructure.Mutators;

/// <summary>
/// Realizes <see cref="IMutationEngine"/>: walks each enabled strategy (gated by the
/// active profile's flag matrix), deduplicates equivalent mutations, rejects no-op or
/// inapplicable candidates, assigns unique ids, and maps test-case coverage from the
/// input keys present on the mutated line.
/// </summary>
public sealed class MutationEngine : IMutationEngine
{
    private readonly IReadOnlyList<IMutationStrategy> _strategies;

    public MutationEngine()
        : this(new IMutationStrategy[]
        {
            new LogicalOperatorMutationStrategy(),
            new ArithmeticOperatorMutationStrategy(),
            new ConstantMutationStrategy(),
            new ComplexExpressionMutationStrategy()
        })
    {
    }

    public MutationEngine(IReadOnlyList<IMutationStrategy> strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    public IReadOnlyList<Mutation> GenerateMutations(
        CobolProgram program,
        MutationProfile profile,
        IReadOnlyList<TestCase>? testCases = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(profile);

        if (program.Ast is null)
        {
            return Array.Empty<Mutation>();
        }

        var candidates = new List<Mutation>();
        foreach (var strategy in _strategies)
        {
            if (!IsEnabled(strategy, profile))
            {
                continue;
            }

            candidates.AddRange(strategy.Apply(program));
        }

        var unique = candidates
            .Distinct(MutationComparer.Instance)
            .Where(mutation => ValidateMutation(program, mutation))
            .ToList();

        var mutations = new List<Mutation>(unique.Count);
        for (var index = 0; index < unique.Count; index++)
        {
            var candidate = unique[index];
            var coveringTestIds = MapCoveringTestIds(program, candidate, testCases);
            mutations.Add(new Mutation(
                $"MUT-{program.Name}-{index + 1:D3}",
                candidate.Type,
                candidate.Line,
                candidate.Original,
                candidate.Mutated,
                coveringTestIds));
        }

        return mutations;
    }

    public bool ValidateMutation(CobolProgram program, Mutation mutation)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(mutation);

        if (string.Equals(mutation.Original, mutation.Mutated, StringComparison.Ordinal))
        {
            return false;
        }

        var lines = program.SourceText.Split('\n');
        if (mutation.Line < 1 || mutation.Line > lines.Length)
        {
            return false;
        }

        return lines[mutation.Line - 1].Contains(mutation.Original, StringComparison.Ordinal);
    }

    public string ApplyMutation(CobolProgram program, Mutation mutation)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(mutation);

        var lines = program.SourceText.Split('\n');
        if (mutation.Line < 1 || mutation.Line > lines.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mutation), "The mutation line is outside the program source.");
        }

        var lineIndex = mutation.Line - 1;
        var line = lines[lineIndex];
        var index = line.IndexOf(mutation.Original, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Original text '{mutation.Original}' was not found on line {mutation.Line}.");
        }

        lines[lineIndex] = line.Remove(index, mutation.Original.Length).Insert(index, mutation.Mutated);
        return string.Join('\n', lines);
    }

    private static bool IsEnabled(IMutationStrategy strategy, MutationProfile profile)
        => strategy.OperationType switch
        {
            OperationType.Logical => profile.LogicalOperators,
            OperationType.Arithmetic => profile.ArithmeticOperators,
            OperationType.ComplexExpression => profile.ComplexExpressions,
            OperationType.Constant => profile.NumericConstants || profile.StringConstants,
            _ => false
        };

    private static IReadOnlyList<string> MapCoveringTestIds(
        CobolProgram program, Mutation mutation, IReadOnlyList<TestCase>? testCases)
    {
        if (testCases is null || testCases.Count == 0)
        {
            return Array.Empty<string>();
        }

        var line = AstTraversal.GetLine(program.SourceText, mutation.Line);
        return testCases
            .Where(testCase => testCase.Inputs.Keys.Any(key => line.Contains(key, StringComparison.OrdinalIgnoreCase)))
            .Select(testCase => testCase.Id)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private sealed class MutationComparer : IEqualityComparer<Mutation>
    {
        public static readonly MutationComparer Instance = new();

        public bool Equals(Mutation? x, Mutation? y)
            => x is not null && y is not null
                && x.Type == y.Type
                && x.Line == y.Line
                && x.Original == y.Original
                && x.Mutated == y.Mutated;

        public int GetHashCode(Mutation obj)
            => HashCode.Combine(obj.Type, obj.Line, obj.Original, obj.Mutated);
    }
}
