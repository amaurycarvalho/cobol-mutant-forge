using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.Mutators;

public class MutationEngineTests
{
    private const string IfProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               DISPLAY A\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    private const string ComputeProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    private const string CombinedProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    [Fact]
    public void GenerateMutations_IfWithAnd_UnderMedium_ProducesOrMutant()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.AndToOr && mutation.Mutated == "OR");
    }

    [Fact]
    public void GenerateMutations_IfWithAnd_UnderMedium_ProducesExpectedCountAndContent()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        var andToOr = Assert.Single(mutations, mutation => mutation.Type == MutationType.AndToOr);
        Assert.Equal("AND", andToOr.Original);
        Assert.Equal("OR", andToOr.Mutated);

        var addNot = Assert.Single(mutations, mutation => mutation.Type == MutationType.AddNot);
        Assert.Equal("IF ", addNot.Original);
        Assert.Equal("IF NOT ", addNot.Mutated);
    }

    [Fact]
    public void GenerateMutations_ComputePlus_UnderMedium_ProducesMinusMutant()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(ComputeProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.AddToSubtract && mutation.Mutated == "-");
    }

    [Fact]
    public void GenerateMutations_UnderLow_ProducesOnlyLogicalAndArithmeticMutations()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(CombinedProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Low);

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.True(
            mutation.Type is MutationType.AndToOr
                or MutationType.OrToAnd
                or MutationType.AddNot
                or MutationType.RemoveNot
                or MutationType.AddToSubtract
                or MutationType.SubtractToAdd
                or MutationType.MultiplyToDivide
                or MutationType.DivideToMultiply));
    }

    [Fact]
    public void GenerateMutations_AssignsUniqueMutationIds()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(CombinedProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.NotEmpty(mutations);
        var ids = mutations.Select(mutation => mutation.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(mutations, mutation => Assert.StartsWith("MUT-", mutation.Id));
    }

    [Fact]
    public void GenerateMutations_IdsStartAtSequenceOne()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.Contains(mutations, mutation => mutation.Id == "MUT-SAMPLE-001");
    }

    [Fact]
    public void GenerateMutations_MapsCoveringTestCasesFromInputKeysOnTheLine()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var testCases = new[]
        {
            new TestCase("TC-001", new Dictionary<string, string> { ["A"] = "1", ["B"] = "2" }),
            new TestCase("TC-002", new Dictionary<string, string> { ["X"] = "9" })
        };

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium, testCases);

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.Contains("TC-001", mutation.CoveringTestIds));
        Assert.All(mutations, mutation => Assert.DoesNotContain("TC-002", mutation.CoveringTestIds));
    }

    [Fact]
    public void GenerateMutations_ProgramWithoutAst_ProducesNoMutations()
    {
        var engine = new MutationEngine();
        var program = new CobolProgram("SAMPLE", IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.Empty(mutations);
    }

    [Fact]
    public void GenerateMutations_DeduplicatesEquivalentMutations()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND C = D AND E = F\r\n" +
            "               DISPLAY A\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        // Two AND nodes collapse into one AndToOr and one AddNot mutation.
        Assert.Single(mutations, mutation => mutation.Type == MutationType.AndToOr);
        Assert.Single(mutations, mutation => mutation.Type == MutationType.AddNot);
    }

    [Fact]
    public void ValidateMutation_RejectsNoOpMutation()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "AND");

        Assert.False(engine.ValidateMutation(program, mutation));
    }

    [Fact]
    public void ValidateMutation_RejectsInapplicableMutation()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "OR", "AND");

        Assert.False(engine.ValidateMutation(program, mutation));
    }

    [Fact]
    public void ValidateMutation_AcceptsApplicableMutation()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.True(engine.ValidateMutation(program, mutation));
    }

    [Fact]
    public void ApplyMutation_ReplacesOriginalWithMutatedOnTargetLine()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        var mutated = engine.ApplyMutation(program, mutation);

        Assert.Contains("IF A > B OR C = D", mutated);
        Assert.DoesNotContain("IF A > B AND C = D", mutated);
    }

    [Fact]
    public void ApplyMutation_MissingOriginalText_Throws()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "OR", "AND");

        Assert.Throws<InvalidOperationException>(() => engine.ApplyMutation(program, mutation));
    }

    [Fact]
    public void ApplyMutation_MissingOriginalText_ThrowsWithMessage()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "OR", "AND");

        var exception = Assert.Throws<InvalidOperationException>(() => engine.ApplyMutation(program, mutation));

        Assert.Contains("Original text 'OR' was not found on line 4.", exception.Message);
    }

    [Fact]
    public void ApplyMutation_OriginalAtColumnZero_Applies()
    {
        var engine = new MutationEngine();
        var program = CreateProgram("IF A > B AND C = D");
        var mutation = new Mutation("m1", MutationType.AndToOr, 1, "IF ", "IF NOT ");

        var mutated = engine.ApplyMutation(program, mutation);

        Assert.StartsWith("IF NOT ", mutated);
    }

    [Fact]
    public void ApplyMutation_LineOutsideSource_ThrowsArgumentOutOfRangeException()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 100, "AND", "OR");

        Assert.Throws<ArgumentOutOfRangeException>(() => engine.ApplyMutation(program, mutation));
    }

    [Fact]
    public void ApplyMutation_NullProgram_Throws()
    {
        var engine = new MutationEngine();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.Throws<ArgumentNullException>(() => engine.ApplyMutation(null!, mutation));
    }

    [Fact]
    public void ApplyMutation_NullMutation_Throws()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        Assert.Throws<ArgumentNullException>(() => engine.ApplyMutation(program, null!));
    }

    [Fact]
    public void ValidateMutation_NullProgram_Throws()
    {
        var engine = new MutationEngine();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.Throws<ArgumentNullException>(() => engine.ValidateMutation(null!, mutation));
    }

    [Fact]
    public void ValidateMutation_NullMutation_Throws()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        Assert.Throws<ArgumentNullException>(() => engine.ValidateMutation(program, null!));
    }

    [Fact]
    public void ValidateMutation_LineBeyondSource_ReturnsFalse()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 100, "AND", "OR");

        Assert.False(engine.ValidateMutation(program, mutation));
    }

    [Fact]
    public void ApplyMutation_OutOfRange_ThrowsWithLocationMessage()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var mutation = new Mutation("m1", MutationType.AndToOr, 100, "AND", "OR");

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => engine.ApplyMutation(program, mutation));

        Assert.Contains("outside the program source", exception.Message);
    }

    [Fact]
    public void GenerateMutations_CoverageOnFirstLine_IsMapped()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(
            "       IF A > B AND C = D\r\n" +
            "       END-PROGRAM.\r\n");
        var testCases = new[]
        {
            new TestCase("TC-001", new Dictionary<string, string> { ["A"] = "1" })
        };

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium, testCases);

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.Contains("TC-001", mutation.CoveringTestIds));
    }

    [Fact]
    public void GenerateMutations_CoverageOnLastLine_IsMapped()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var testCases = new[]
        {
            new TestCase("TC-LAST", new Dictionary<string, string> { ["END-PROGRAM"] = "x" })
        };

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium, testCases);

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.DoesNotContain("TC-LAST", mutation.CoveringTestIds));
    }

    [Fact]
    public void GenerateMutations_Comparer_DistinguishesByAllFields()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND C = D\r\n" +
            "           END-IF.\r\n" +
            "           IF A > B AND C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        // Two AND nodes on different lines must both be represented (not deduped).
        Assert.Equal(2, mutations.Count(mutation => mutation.Type == MutationType.AndToOr));
    }

    [Fact]
    public void GenerateMutations_NullProgram_Throws()
    {
        var engine = new MutationEngine();

        Assert.Throws<ArgumentNullException>(() => engine.GenerateMutations(null!, MutationProfile.Medium));
    }

    [Fact]
    public void GenerateMutations_NullProfile_Throws()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        Assert.Throws<ArgumentNullException>(() => engine.GenerateMutations(program, null!));
    }

    [Fact]
    public void Constructor_NullStrategies_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MutationEngine(null!));
    }

    [Fact]
    public void GenerateMutations_WithEmptyStrategyList_ProducesNoMutations()
    {
        var engine = new MutationEngine(Array.Empty<IMutationStrategy>());
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium);

        Assert.Empty(mutations);
    }

    [Fact]
    public void IsEnabled_ComplexExpressionStrategy_GatedByProfileFlag()
    {
        var engine = new MutationEngine(new IMutationStrategy[]
        {
            new FakeStrategy(OperationType.ComplexExpression, produceMutation: true)
        });
        var program = CreateProgram(IfProgram);

        var enabled = engine.GenerateMutations(program, MutationProfile.High);
        var disabled = engine.GenerateMutations(program, MutationProfile.Low);

        Assert.Single(enabled);
        Assert.Empty(disabled);
    }

    [Fact]
    public void IsEnabled_ConstantStrategy_GatedByNumericOrStringFlags()
    {
        var engine = new MutationEngine(new IMutationStrategy[]
        {
            new FakeStrategy(OperationType.Constant, produceMutation: true)
        });
        var program = CreateProgram(IfProgram);

        // Medium enables numeric constants; Low disables both numeric and string.
        var enabled = engine.GenerateMutations(program, MutationProfile.Medium);
        var disabled = engine.GenerateMutations(program, MutationProfile.Low);

        Assert.Single(enabled);
        Assert.Empty(disabled);
    }

    [Fact]
    public void IsEnabled_LogicalStrategy_GatedByLogicalFlag()
    {
        var engine = new MutationEngine(new IMutationStrategy[]
        {
            new FakeStrategy(OperationType.Logical, produceMutation: true)
        });
        var program = CreateProgram(IfProgram);

        var enabled = engine.GenerateMutations(program, MutationProfile.Low);
        var disabled = engine.GenerateMutations(program, MutationProfile.High);

        Assert.Single(enabled);
        Assert.Single(disabled);
    }

    [Fact]
    public void IsEnabled_UnknownOperationType_IsDisabled()
    {
        var engine = new MutationEngine(new IMutationStrategy[]
        {
            new FakeStrategy((OperationType)999, produceMutation: true)
        });
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.High);

        Assert.Empty(mutations);
    }

    [Fact]
    public void GenerateMutations_EmptyTestCases_ProducesNoCoverage()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium, Array.Empty<TestCase>());

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.Empty(mutation.CoveringTestIds));
    }

    [Fact]
    public void GenerateMutations_CoverageMatchingIsCaseInsensitiveAndDistinct()
    {
        var engine = new MutationEngine();
        var program = CreateProgram(IfProgram);
        var testCases = new[]
        {
            new TestCase("TC-001", new Dictionary<string, string> { ["a"] = "1", ["ZZZ"] = "9" }),
            new TestCase("TC-002", new Dictionary<string, string> { ["XYZ"] = "9" })
        };

        var mutations = engine.GenerateMutations(program, MutationProfile.Medium, testCases);

        Assert.NotEmpty(mutations);
        Assert.All(mutations, mutation => Assert.Equal(new[] { "TC-001" }, mutation.CoveringTestIds));
    }


    private sealed class FakeStrategy : IMutationStrategy
    {
        private readonly OperationType _operationType;
        private readonly bool _produceMutation;

        public FakeStrategy(OperationType operationType, bool produceMutation = false)
        {
            _operationType = operationType;
            _produceMutation = produceMutation;
        }

        public MutationType MutationType => MutationType.AndToOr;

        public OperationType OperationType => _operationType;

        public IReadOnlyList<Mutation> Apply(CobolProgram program)
            => _produceMutation
                ? new[] { new Mutation("tmp", MutationType.AndToOr, 4, "AND", "OR") }
                : Array.Empty<Mutation>();
    }

    private static CobolProgram CreateProgram(string source)
    {
        var parser = new TypeCobolParserAdapter();
        var result = parser.Parse(source);
        return new CobolProgram("SAMPLE", source, null, result.Ast);
    }
}
