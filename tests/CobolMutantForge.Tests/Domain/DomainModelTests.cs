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
        Assert.Throws<ArgumentOutOfRangeException>(() => new Mutation(
            "mut-1", MutationType.AndToOr, line: 0, "orig", "mut"));
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
}
