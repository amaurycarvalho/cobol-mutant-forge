using System.Text.Json;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Exporters;
using Xunit;

namespace CobolMutantForge.Tests.Exporters;

public class PackageManifestReaderTests
{
    [Fact]
    public void Read_ReconstructsPackageFromManifest()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            exporter.Export(CreatePackage(), directory);
            var manifestPath = Path.Combine(
                Directory.EnumerateDirectories(directory).Single(), "manifest.json");
            var reader = new PackageManifestReader();

            var package = reader.Read(manifestPath);

            Assert.NotNull(package.SourceProgram);
            Assert.Equal("PAYMENT", package.SourceProgram!.Name);
            var mutation = Assert.Single(package.Mutants);
            Assert.Equal(MutationType.AndToOr, mutation.Type);
            Assert.Equal("AND", mutation.Original);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_MissingSourceFile_ReturnsPackageWithoutProgram()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            exporter.Export(CreatePackage(), directory);
            var packageDirectory = Directory.EnumerateDirectories(directory).Single();
            File.Delete(Path.Combine(packageDirectory, "PAYMENT.cbl"));

            var package = new PackageManifestReader().Read(Path.Combine(packageDirectory, "manifest.json"));

            Assert.Null(package.SourceProgram);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ReadAll_CollectsOnlyFoldersWithManifest()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            exporter.Export(CreatePackage(), directory);
            Directory.CreateDirectory(Path.Combine(directory, "no-manifest"));

            var packages = new PackageManifestReader().ReadAll(directory);

            Assert.Single(packages);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ReadAll_BlankSourceDirectory_Throws()
    {
        var reader = new PackageManifestReader();

        Assert.Throws<ArgumentException>(() => reader.ReadAll(" "));
    }

    [Fact]
    public void Read_BlankManifestPath_Throws()
    {
        var reader = new PackageManifestReader();

        Assert.Throws<ArgumentException>(() => reader.Read(" "));
    }

    [Fact]
    public void Read_MalformedManifestJson_ThrowsJsonException()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            var manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath, "{not valid json");

            Assert.Throws<JsonException>(() => new PackageManifestReader().Read(manifestPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_NullManifestJson_ThrowsInvalidDataException()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            var manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath, "null");

            var exception = Assert.Throws<InvalidDataException>(() => new PackageManifestReader().Read(manifestPath));

            Assert.Contains("Failed to deserialize the manifest.", exception.Message);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_ReconstructsSourceProgramAndId()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            exporter.Export(CreatePackage(), directory);
            var manifestPath = Path.Combine(
                Directory.EnumerateDirectories(directory).Single(), "manifest.json");

            var package = new PackageManifestReader().Read(manifestPath);

            Assert.NotNull(package.SourceProgram);
            Assert.Equal("PAYMENT", package.SourceProgram!.Name);
            Assert.Equal("MUT-001-PAYMENT-001", package.Id);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_RoundTripsProfileAndTestCaseCoverage()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            var program = new CobolProgram("PAYMENT", "IF A > B AND C = D");
            var package = new MutantPackage("PAYMENT", program) { Profile = MutationProfile.Medium };
            package.AddMutant(new Mutation("MUT-PAYMENT-001", MutationType.AndToOr, 1, "AND", "OR", new[] { "TC-001" }));
            exporter.Export(package, directory);
            var manifestPath = Path.Combine(
                Directory.EnumerateDirectories(directory).Single(), "manifest.json");

            var roundTripped = new PackageManifestReader().Read(manifestPath);

            Assert.Equal(MutationProfile.Medium, roundTripped.Profile);
            var mutation = Assert.Single(roundTripped.Mutants);
            Assert.Equal(new[] { "TC-001" }, mutation.CoveringTestIds);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_RoundTripsAllMutationTypes()
    {
        var directory = CreateTempDirectory();
        try
        {
            var exporter = new MutantPackageExporter(ExportFormat.Folder);
            var program = new CobolProgram("PAYMENT",
                "IF A > B AND C = D\r\n" +
                "COMPUTE T = X + Y\r\n" +
                "MOVE 1 TO A\r\n");
            var package = new MutantPackage("PAYMENT", program) { Profile = MutationProfile.High };
            package.AddMutant(new Mutation("m1", MutationType.AndToOr, 1, "AND", "OR"));
            package.AddMutant(new Mutation("m2", MutationType.AddToSubtract, 2, "+", "-"));
            package.AddMutant(new Mutation("m3", MutationType.ConstantReplacement, 3, "1", "2"));
            package.AddMutant(new Mutation("m4", MutationType.ComplexExpressionMutation, 1, "A", "B"));
            exporter.Export(package, directory);
            var manifestPath = Path.Combine(
                Directory.EnumerateDirectories(directory).Single(), "manifest.json");

            var roundTripped = new PackageManifestReader().Read(manifestPath);

            Assert.Equal(4, roundTripped.Mutants.Count);
            Assert.Contains(roundTripped.Mutants, mutation => mutation.Type == MutationType.AndToOr);
            Assert.Contains(roundTripped.Mutants, mutation => mutation.Type == MutationType.AddToSubtract);
            Assert.Contains(roundTripped.Mutants, mutation => mutation.Type == MutationType.ConstantReplacement);
            Assert.Contains(roundTripped.Mutants, mutation => mutation.Type == MutationType.ComplexExpressionMutation);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Read_UnknownMutationType_DefaultsToConstantReplacement()
    {
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            var manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath,
                """{"mutantId":"MUT-001","originalProgram":"PAYMENT","mutationProfile":"medium","mutations":[{"id":"m1","type":"banana","line":1,"original":"AND","mutated":"OR"}]}""");

            var package = new PackageManifestReader().Read(manifestPath);

            var mutation = Assert.Single(package.Mutants);
            Assert.Equal(MutationType.ConstantReplacement, mutation.Type);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static MutantPackage CreatePackage()
    {
        var program = new CobolProgram("PAYMENT", "IF A > B AND C = D");
        var package = new MutantPackage("PAYMENT", program) { Profile = MutationProfile.Medium };
        package.AddMutant(new Mutation("MUT-PAYMENT-001", MutationType.AndToOr, 1, "AND", "OR"));
        return package;
    }

    private static string CreateTempDirectory()
        => Path.Combine(Path.GetTempPath(), "cmf-reader-" + Guid.NewGuid().ToString("N"));
}
