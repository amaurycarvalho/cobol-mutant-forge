using CobolMutantForge.Application.Services;
using CobolMutantForge.Application.UseCases;
using CobolMutantForge.Domain.Aggregates;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.Application;

public class GenerateMutationsUseCaseTests
{
    private const string Source =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. PAYMENT.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM PAYMENT.\r\n";

    [Fact]
    public void Execute_ProducesPackagesWithValidatedMutations()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.Medium);
        project.AddProgram(CreateProgram(Source));

        var result = useCase.Execute(project);

        var package = Assert.Single(result.Packages);
        Assert.Equal("PAYMENT", package.Id);
        Assert.NotEmpty(package.Mutants);
        Assert.Contains(package.Mutants, mutation =>
            mutation.Type == MutationType.AndToOr && mutation.Mutated == "OR");
        Assert.Contains(package.Mutants, mutation =>
            mutation.Type == MutationType.AddToSubtract && mutation.Mutated == "-");
    }

    [Fact]
    public void Execute_ParsesProgramWithoutAst()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.Medium);
        project.AddProgram(new CobolProgram("PAYMENT", Source));

        var result = useCase.Execute(project);

        var package = Assert.Single(result.Packages);
        Assert.NotEmpty(package.Mutants);
    }

    [Fact]
    public void Execute_LowProfile_ProducesOnlyLogicalMutations()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.Low);
        project.AddProgram(CreateProgram(Source));

        var result = useCase.Execute(project);

        var package = Assert.Single(result.Packages);
        Assert.All(package.Mutants, mutation =>
            Assert.NotEqual(MutationType.ConstantReplacement, mutation.Type));
    }

    [Fact]
    public void Execute_NullProject_Throws()
    {
        var useCase = CreateUseCase();

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
    }

    [Fact]
    public void Execute_EmptyProject_ProducesNoPackagesAndNoWarnings()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample");

        var result = useCase.Execute(project);

        Assert.Empty(result.Packages);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Execute_ProgramWithAst_IsNotReparsed()
    {
        var useCase = CreateUseCase();
        var parsed = CreateProgram(Source);
        var project = new MutationProject("sample", profile: MutationProfile.Medium);
        project.AddProgram(parsed);

        var result = useCase.Execute(project);

        var package = Assert.Single(result.Packages);
        Assert.Same(parsed.Ast, package.SourceProgram!.Ast);
    }

    [Fact]
    public void Execute_ParseFailure_AddsWarning()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.Medium);
        project.AddProgram(new CobolProgram("BROKEN", "'unterminated string"));

        var result = useCase.Execute(project);

        Assert.Contains(result.Warnings, warning =>
            warning.Contains("BROKEN", StringComparison.OrdinalIgnoreCase));
        Assert.Single(result.Packages);
    }

    [Fact]
    public void Execute_CleanProgram_ProducesNoWarnings()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.Medium);
        project.AddProgram(CreateProgram(Source));

        var result = useCase.Execute(project);

        Assert.Empty(result.Warnings);
        var package = Assert.Single(result.Packages);
        Assert.NotEmpty(package.Mutants);
    }

    [Fact]
    public void Execute_ProfileIsCarriedToPackages()
    {
        var useCase = CreateUseCase();
        var project = new MutationProject("sample", profile: MutationProfile.High);
        project.AddProgram(CreateProgram(Source));

        var result = useCase.Execute(project);

        var package = Assert.Single(result.Packages);
        Assert.Equal(MutationProfile.High, package.Profile);
    }

    [Fact]
    public void ValidationService_RejectsNoOpMutation()
    {
        var service = new ValidationService();
        var program = CreateProgram(Source);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "AND");

        Assert.False(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void ValidationService_RejectsInapplicableMutation()
    {
        var service = new ValidationService();
        var program = CreateProgram(Source);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "OR", "AND");

        Assert.False(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void ValidationService_AcceptsApplicableMutation()
    {
        var service = new ValidationService();
        var program = CreateProgram(Source);
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.True(service.IsApplicable(program, mutation));
    }

    private static GenerateMutationsUseCase CreateUseCase()
        => new(new MutationEngine(), new TypeCobolParserAdapter(), new ValidationService());

    private static CobolProgram CreateProgram(string source)
    {
        var parser = new TypeCobolParserAdapter();
        var result = parser.Parse(source);
        return new CobolProgram("PAYMENT", source, null, result.Ast);
    }
}
