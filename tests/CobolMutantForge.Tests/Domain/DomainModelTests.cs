using CobolMutantForge.Domain.Aggregates;
using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Domain.ValueObjects;
using Xunit;

namespace CobolMutantForge.Tests.Domain;

public class MutationProfileTests
{
    [Fact]
    public void FromName_AcceptsLowMediumHigh()
    {
        Assert.Equal(MutationProfile.Low, MutationProfile.FromName("low"));
        Assert.Equal(MutationProfile.Medium, MutationProfile.FromName("medium"));
        Assert.Equal(MutationProfile.High, MutationProfile.FromName("high"));
    }

    [Fact]
    public void FromName_IgnoresCase()
    {
        Assert.Equal(MutationProfile.Medium, MutationProfile.FromName("MEDIUM"));
    }

    [Fact]
    public void FromName_ThrowsOnUnknownProfile()
    {
        Assert.Throws<ArgumentException>(() => MutationProfile.FromName("extreme"));
    }

    [Fact]
    public void FromName_ThrowsOnBlank()
    {
        Assert.Throws<ArgumentException>(() => MutationProfile.FromName(" "));
    }

    [Fact]
    public void FromName_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => MutationProfile.FromName(null!));
    }

    [Fact]
    public void FromName_TrimsWhitespaceAroundName()
    {
        Assert.Equal(MutationProfile.High, MutationProfile.FromName("  high  "));
    }

    [Fact]
    public void FromName_ErrorMessageMentionsValidProfiles()
    {
        var exception = Assert.Throws<ArgumentException>(() => MutationProfile.FromName("extreme"));

        Assert.Contains("low, medium, high", exception.Message);
    }

    [Fact]
    public void LowProfile_HasExpectedName()
    {
        Assert.Equal("low", MutationProfile.Low.Name);
    }

    [Fact]
    public void MediumProfile_HasExpectedName()
    {
        Assert.Equal("medium", MutationProfile.Medium.Name);
    }

    [Fact]
    public void HighProfile_HasExpectedName()
    {
        Assert.Equal("high", MutationProfile.High.Name);
    }

    [Fact]
    public void MediumProfile_MatchesPdrMatrix()
    {
        Assert.True(MutationProfile.Medium.LogicalOperators);
        Assert.True(MutationProfile.Medium.ArithmeticOperators);
        Assert.False(MutationProfile.Medium.ComplexExpressions);
        Assert.True(MutationProfile.Medium.NumericConstants);
        Assert.False(MutationProfile.Medium.StringConstants);
    }

    [Fact]
    public void LowProfile_EnablesOnlyLogicalOperators()
    {
        Assert.True(MutationProfile.Low.LogicalOperators);
        Assert.False(MutationProfile.Low.ArithmeticOperators);
        Assert.False(MutationProfile.Low.ComplexExpressions);
        Assert.False(MutationProfile.Low.NumericConstants);
        Assert.False(MutationProfile.Low.StringConstants);
    }

    [Fact]
    public void HighProfile_EnablesAllFlags()
    {
        Assert.True(MutationProfile.High.LogicalOperators);
        Assert.True(MutationProfile.High.ArithmeticOperators);
        Assert.True(MutationProfile.High.ComplexExpressions);
        Assert.True(MutationProfile.High.NumericConstants);
        Assert.True(MutationProfile.High.StringConstants);
    }
}

public class MutationTypeTests
{
    [Theory]
    [InlineData(MutationType.AndToOr)]
    [InlineData(MutationType.OrToAnd)]
    [InlineData(MutationType.AddNot)]
    [InlineData(MutationType.RemoveNot)]
    [InlineData(MutationType.AddToSubtract)]
    [InlineData(MutationType.SubtractToAdd)]
    [InlineData(MutationType.MultiplyToDivide)]
    [InlineData(MutationType.DivideToMultiply)]
    [InlineData(MutationType.ConstantReplacement)]
    public void AllMutationTypes_AreDefined(MutationType type)
    {
        Assert.True(Enum.IsDefined(type));
    }
}

public class MutationTests
{
    [Fact]
    public void Mutation_ExposesOriginalMutatedAndLine()
    {
        var mutation = new Mutation(
            "mut-1",
            MutationType.AndToOr,
            line: 42,
            original: "IF A AND B",
            mutated: "IF A OR B");

        Assert.Equal("mut-1", mutation.Id);
        Assert.Equal(MutationType.AndToOr, mutation.Type);
        Assert.Equal(42, mutation.Line);
        Assert.Equal("IF A AND B", mutation.Original);
        Assert.Equal("IF A OR B", mutation.Mutated);
    }

    [Fact]
    public void Mutation_ThrowsOnNonPositiveLine()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Mutation(
            "mut-1", MutationType.AndToOr, line: 0, "orig", "mut"));

        Assert.Contains("The mutation line must be a positive line number.", exception.Message);
    }

    [Fact]
    public void Mutation_ThrowsOnNullOriginal()
    {
        Assert.Throws<ArgumentNullException>(() => new Mutation(
            "mut-1", MutationType.AndToOr, 1, null!, "mut"));
    }

    [Fact]
    public void Mutation_ThrowsOnNullMutated()
    {
        Assert.Throws<ArgumentNullException>(() => new Mutation(
            "mut-1", MutationType.AndToOr, 1, "orig", null!));
    }

    [Fact]
    public void Mutation_DefaultsCoveringTestIdsToEmpty()
    {
        var mutation = new Mutation("mut-1", MutationType.AndToOr, 1, "orig", "mut");

        Assert.Empty(mutation.CoveringTestIds);
    }

    [Fact]
    public void Mutation_EqualityComparesOnlyId()
    {
        var a = new Mutation("mut-1", MutationType.AndToOr, 1, "A AND B", "A OR B");
        var b = new Mutation("mut-1", MutationType.OrToAnd, 99, "different", "text");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Mutation_WithDifferentId_AreNotEqual()
    {
        var a = new Mutation("mut-1", MutationType.AndToOr, 1, "A AND B", "A OR B");
        var b = new Mutation("mut-2", MutationType.AndToOr, 1, "A AND B", "A OR B");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Mutation_EqualsNull_IsFalse()
    {
        var mutation = new Mutation("mut-1", MutationType.AndToOr, 1, "orig", "mut");

        Assert.False(mutation.Equals(null));
        Assert.False(mutation.Equals((object?)null));
    }

    [Fact]
    public void Mutation_EqualsNonMutation_IsFalse()
    {
        var mutation = new Mutation("mut-1", MutationType.AndToOr, 1, "orig", "mut");

        Assert.False(mutation.Equals("mut-1"));
    }
}

public class CobolProgramTests
{
    [Fact]
    public void ProgramsWithSameNameAndSourceHash_AreEqual()
    {
        const string source = "PROGRAM-ID. HELLO.";

        var a = new CobolProgram("HELLO", source);
        var b = new CobolProgram("HELLO", source);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ProgramsWithDifferentSource_AreNotEqual()
    {
        var a = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");
        var b = new CobolProgram("HELLO", "PROGRAM-ID. WORLD.");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ProgramsWithDifferentName_AreNotEqual()
    {
        var a = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");
        var b = new CobolProgram("WORLD", "PROGRAM-ID. HELLO.");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Program_EqualsNull_IsFalse()
    {
        var program = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");

        Assert.False(program.Equals(null));
    }

    [Fact]
    public void Program_ThrowsOnBlankName()
    {
        Assert.Throws<ArgumentException>(() => new CobolProgram(" ", "PROGRAM-ID. HELLO."));
    }

    [Fact]
    public void Program_ThrowsOnNullSourceText()
    {
        Assert.Throws<ArgumentNullException>(() => new CobolProgram("HELLO", null!));
    }

    [Fact]
    public void Program_DefaultsCopybooksToEmpty()
    {
        var program = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");

        Assert.Empty(program.Copybooks);
    }

    [Fact]
    public void Program_ComputesDeterministicSourceHash()
    {
        var a = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");
        var b = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");

        Assert.Equal(a.SourceHash, b.SourceHash);
        Assert.NotEmpty(a.SourceHash);
    }

    [Fact]
    public void Program_DifferentSource_ProducesDifferentHash()
    {
        var a = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");
        var b = new CobolProgram("HELLO", "PROGRAM-ID. WORLD.");

        Assert.NotEqual(a.SourceHash, b.SourceHash);
    }
}

public class MutantPackageTests
{
    [Fact]
    public void AddMutant_ExposesCollectionAndCount()
    {
        var package = new MutantPackage("pkg-1");
        package.AddMutant(new Mutation("m1", MutationType.AndToOr, 1, "a AND b", "a OR b"));
        package.AddMutant(new Mutation("m2", MutationType.OrToAnd, 2, "a OR b", "a AND b"));

        Assert.Equal(2, package.Count);
        Assert.Equal(2, package.Mutants.Count);
    }

    [Fact]
    public void AddMutant_ThrowsOnNull()
    {
        var package = new MutantPackage("pkg-1");

        Assert.Throws<ArgumentNullException>(() => package.AddMutant(null!));
    }

    [Fact]
    public void MutantPackage_EqualityComparesOnlyId()
    {
        var a = new MutantPackage("pkg-1");
        var b = new MutantPackage("pkg-1");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void MutantPackage_WithDifferentId_AreNotEqual()
    {
        var a = new MutantPackage("pkg-1");
        var b = new MutantPackage("pkg-2");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void MutantPackage_EqualsNull_IsFalse()
    {
        var package = new MutantPackage("pkg-1");

        Assert.False(package.Equals(null));
    }
}

public class MutationProjectTests
{
    [Fact]
    public void Project_ExposesProgramsTestCasesAndProfile()
    {
        var project = new MutationProject(
            "sample",
            new Dictionary<string, string> { ["sourceDirectory"] = "src" },
            MutationProfile.Medium);

        project.AddProgram(new CobolProgram("HELLO", "PROGRAM-ID. HELLO."));
        project.AddTestCase(new TestCase("tc-1"));

        Assert.Equal("sample", project.ProjectName);
        Assert.Equal("src", project.Paths["sourceDirectory"]);
        Assert.Equal(MutationProfile.Medium, project.Profile);
        Assert.Single(project.Programs);
        Assert.Single(project.TestCases);
    }

    [Fact]
    public void Project_DefaultsPathsToEmptyAndProfileToMedium()
    {
        var project = new MutationProject("sample");

        Assert.Empty(project.Paths);
        Assert.Equal(MutationProfile.Medium, project.Profile);
    }

    [Fact]
    public void Project_ThrowsOnBlankName()
    {
        Assert.Throws<ArgumentException>(() => new MutationProject(" "));
    }

    [Fact]
    public void Project_AddProgram_ThrowsOnNull()
    {
        var project = new MutationProject("sample");

        Assert.Throws<ArgumentNullException>(() => project.AddProgram(null!));
    }

    [Fact]
    public void Project_AddTestCase_ThrowsOnNull()
    {
        var project = new MutationProject("sample");

        Assert.Throws<ArgumentNullException>(() => project.AddTestCase(null!));
    }

    [Fact]
    public void Project_StartsWithNoProgramsOrTestCases()
    {
        var project = new MutationProject("sample");

        Assert.Empty(project.Programs);
        Assert.Empty(project.TestCases);
    }

    [Fact]
    public void Project_ExplicitProfile_IsUsed()
    {
        var project = new MutationProject("sample", profile: MutationProfile.High);

        Assert.Equal(MutationProfile.High, project.Profile);
    }
}

public class ImportResultTests
{
    [Fact]
    public void Defaults_AreEmptyAndValid()
    {
        var result = new ImportResult();

        Assert.Empty(result.Programs);
        Assert.Empty(result.TestCases);
        Assert.Empty(result.Warnings);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void PopulatedValues_AreExposed()
    {
        var result = new ImportResult
        {
            Programs = new[] { new CobolProgram("HELLO", "PROGRAM-ID. HELLO.") },
            TestCases = new[] { new TestCase("tc-1") },
            Warnings = new[] { "warn" },
            IsValid = false
        };

        Assert.Single(result.Programs);
        Assert.Single(result.TestCases);
        Assert.Equal("warn", result.Warnings[0]);
        Assert.False(result.IsValid);
    }
}

public class TestCaseDataTests
{
    [Fact]
    public void TestCase_ExposesInputsAndExpectedOutputs()
    {
        var testCase = new TestCase(
            "tc-1",
            new Dictionary<string, string> { ["A"] = "1" },
            new Dictionary<string, string> { ["B"] = "2" });

        Assert.Equal("1", testCase.Inputs["A"]);
        Assert.Equal("2", testCase.ExpectedOutputs["B"]);
    }

    [Fact]
    public void TestCase_DefaultsInputsAndExpectedOutputsToEmpty()
    {
        var testCase = new TestCase("tc-1");

        Assert.Empty(testCase.Inputs);
        Assert.Empty(testCase.ExpectedOutputs);
    }

    [Fact]
    public void TestCase_ThrowsOnBlankId()
    {
        Assert.Throws<ArgumentException>(() => new TestCase(" "));
    }

    [Fact]
    public void TestCase_EqualityComparesOnlyId()
    {
        var a = new TestCase("tc-1", new Dictionary<string, string> { ["A"] = "1" });
        var b = new TestCase("tc-1", new Dictionary<string, string> { ["B"] = "2" });

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TestCase_WithDifferentId_AreNotEqual()
    {
        var a = new TestCase("tc-1");
        var b = new TestCase("tc-2");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TestCase_EqualsNull_IsFalse()
    {
        var testCase = new TestCase("tc-1");

        Assert.False(testCase.Equals(null));
    }
}

public class MutantPackageDetailsTests
{
    [Fact]
    public void MutantPackage_ExposesManifestReportAndSourceProgram()
    {
        var program = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");
        var package = new MutantPackage("pkg-1", program)
        {
            Manifest = "{}",
            Report = "{}"
        };

        Assert.Equal("{}", package.Manifest);
        Assert.Equal("{}", package.Report);
        Assert.Same(program, package.SourceProgram);
    }

    [Fact]
    public void MutantPackage_ThrowsOnBlankId()
    {
        Assert.Throws<ArgumentException>(() => new MutantPackage(" "));
    }

    [Fact]
    public void MutantPackage_DefaultsToMediumProfile()
    {
        var package = new MutantPackage("pkg-1");

        Assert.Equal(MutationProfile.Medium, package.Profile);
    }

    [Fact]
    public void MutantPackage_StartsWithNoMutants()
    {
        var package = new MutantPackage("pkg-1");

        Assert.Equal(0, package.Count);
        Assert.Empty(package.Mutants);
        Assert.Null(package.SourceProgram);
        Assert.Null(package.Manifest);
        Assert.Null(package.Report);
    }
}

public class MutationDetailsTests
{
    [Fact]
    public void Mutation_ExposesCoveringTestIds()
    {
        var mutation = new Mutation("m1", MutationType.AndToOr, 1, "a", "b", new[] { "tc-1", "tc-2" });

        Assert.Contains("tc-1", mutation.CoveringTestIds);
        Assert.Equal(2, mutation.CoveringTestIds.Count);
    }

    [Fact]
    public void Mutation_ThrowsOnBlankId()
    {
        Assert.Throws<ArgumentException>(() => new Mutation("", MutationType.AndToOr, 1, "a", "b"));
    }

    [Fact]
    public void Mutation_GetHashCode_IsStableAcrossEquivalentInstances()
    {
        var a = new Mutation("m1", MutationType.AndToOr, 1, "a", "b");
        var b = new Mutation("m1", MutationType.OrToAnd, 2, "c", "d");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}

public class CobolProgramDetailsTests
{
    [Fact]
    public void CobolProgram_ExposesCopybooksAndAst()
    {
        var ast = new AstNode { Kind = "Program" };
        var program = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.", new[] { "CPY1", "CPY2" }, ast);

        Assert.Equal(2, program.Copybooks.Count);
        Assert.Same(ast, program.Ast);
    }

    [Fact]
    public void CobolProgram_WithNullAst_ExposesNullAst()
    {
        var program = new CobolProgram("HELLO", "PROGRAM-ID. HELLO.");

        Assert.Null(program.Ast);
    }
}

public class AstNodeTests
{
    [Fact]
    public void AstNode_DefaultsAreEmpty()
    {
        var node = new AstNode();

        Assert.Equal(string.Empty, node.Kind);
        Assert.Equal(string.Empty, node.Text);
        Assert.Equal(0, node.Line);
        Assert.Equal(0, node.Column);
        Assert.Empty(node.Children);
    }

    [Fact]
    public void AstNode_ExposesPopulatedValues()
    {
        var child = new AstNode { Kind = "ArithmeticOperator", Text = "+", Line = 4, Column = 30 };
        var node = new AstNode
        {
            Kind = "Program",
            Text = "SAMPLE",
            Line = 1,
            Column = 1,
            Children = new[] { child }
        };

        Assert.Equal("Program", node.Kind);
        Assert.Equal("SAMPLE", node.Text);
        Assert.Equal(1, node.Line);
        Assert.Equal(1, node.Column);
        var descendant = Assert.Single(node.Children);
        Assert.Equal("+", descendant.Text);
    }

    [Fact]
    public void AstNode_RecordEquality_ComparesAllProperties()
    {
        var a = new AstNode { Kind = "Program", Line = 1 };
        var b = new AstNode { Kind = "Program", Line = 1 };
        var c = new AstNode { Kind = "Program", Line = 2 };

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }
}

public class ParseResultTests
{
    [Fact]
    public void ParseResult_Defaults_HasNoErrorsAndEmptyAst()
    {
        var result = new ParseResult();

        Assert.Equal(string.Empty, result.Ast.Kind);
        Assert.Empty(result.Diagnostics);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void ParseResult_HasErrors_WhenAnErrorDiagnosticExists()
    {
        var result = new ParseResult
        {
            Diagnostics = new[]
            {
                new ParseDiagnostic(DiagnosticSeverity.Error, "boom", 4, 17)
            }
        };

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void ParseResult_DoesNotHaveErrors_WhenOnlyWarningsExist()
    {
        var result = new ParseResult
        {
            Diagnostics = new[]
            {
                new ParseDiagnostic(DiagnosticSeverity.Warning, "warn", 4, 17)
            }
        };

        Assert.False(result.HasErrors);
    }

    [Fact]
    public void ParseDiagnostic_ExposesAllFields()
    {
        var diagnostic = new ParseDiagnostic(DiagnosticSeverity.Error, "boom", 4, 17);

        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("boom", diagnostic.Message);
        Assert.Equal(4, diagnostic.Line);
        Assert.Equal(17, diagnostic.Column);
    }
}
