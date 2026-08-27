using CobolMutantForge.Application.UseCases;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Exporters;
using Xunit;

namespace CobolMutantForge.Tests.Application;

public class ExportMutantsUseCaseTests
{
    [Fact]
    public void Execute_ExportsEveryPackageIntoTheOutputDirectory()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var useCase = new ExportMutantsUseCase(exporter);
        var packages = new[] { CreatePackage("PAYMENT"), CreatePackage("ACCOUNT") };
        var outputDirectory = Path.Combine(Path.GetTempPath(), "cmf-uc-" + Guid.NewGuid().ToString("N"));
        try
        {
            useCase.Execute(packages, outputDirectory);

            Assert.Equal(2, Directory.EnumerateDirectories(outputDirectory).Count());
            Assert.All(packages, package => Assert.NotNull(package.Manifest));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Execute_NullPackages_Throws()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var useCase = new ExportMutantsUseCase(exporter);

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!, "out"));
    }

    [Fact]
    public void Execute_BlankOutputDirectory_Throws()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var useCase = new ExportMutantsUseCase(exporter);
        var packages = new[] { CreatePackage("PAYMENT") };

        Assert.Throws<ArgumentException>(() => useCase.Execute(packages, " "));
    }

    [Fact]
    public void Execute_EmptyPackages_DoesNothing()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var useCase = new ExportMutantsUseCase(exporter);
        var outputDirectory = Path.Combine(Path.GetTempPath(), "cmf-uc-" + Guid.NewGuid().ToString("N"));
        try
        {
            useCase.Execute(Array.Empty<MutantPackage>(), outputDirectory);

            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void Execute_BlankOutputDirectoryWithNoPackages_Throws()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var useCase = new ExportMutantsUseCase(exporter);

        Assert.Throws<ArgumentException>(() => useCase.Execute(Array.Empty<MutantPackage>(), " "));
    }

    private static MutantPackage CreatePackage(string name)
    {
        var program = new CobolProgram(name, "IF A AND B");
        var package = new MutantPackage(name, program) { Profile = MutationProfile.Low };
        package.AddMutant(new Mutation("MUT-1", MutationType.AndToOr, 1, "AND", "OR"));
        return package;
    }
}
