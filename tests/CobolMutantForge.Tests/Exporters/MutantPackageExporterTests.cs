using System.IO.Compression;
using System.Text.Json;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Exporters;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.Exporters;

public class MutantPackageExporterTests
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
    public void Export_Zip_ArchiveContainsCblManifestAndReport()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Zip);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var zipPath = Directory.EnumerateFiles(outputDirectory, "*.zip").Single();
            using var archive = ZipFile.OpenRead(zipPath);
            var entries = archive.Entries.Select(entry => entry.FullName).ToList();

            Assert.Contains(entries, entry => entry.EndsWith(".cbl", StringComparison.OrdinalIgnoreCase));
            Assert.Contains("manifest.json", entries);
            Assert.Contains("mutations-report.json", entries);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_Folder_WritesCblManifestAndReport()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var files = Directory.EnumerateFiles(targetDirectory).Select(Path.GetFileName).ToList();

            Assert.Contains(files, file => file is not null && file.EndsWith(".cbl", StringComparison.OrdinalIgnoreCase));
            Assert.Contains("manifest.json", files);
            Assert.Contains("mutations-report.json", files);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Manifest_ContainsAllPdrDefinedFields()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("mutantId", out _));
            Assert.True(root.TryGetProperty("originalProgram", out _));
            Assert.True(root.TryGetProperty("baseProgramHash", out _));
            Assert.True(root.TryGetProperty("timestamp", out _));
            Assert.True(root.TryGetProperty("mutationProfile", out _));
            Assert.True(root.TryGetProperty("sourceCopied", out _));
            Assert.True(root.TryGetProperty("copybooksResolved", out _));
            Assert.True(root.TryGetProperty("mutations", out var mutations));

            var mutation = mutations.EnumerateArray().First();
            Assert.True(mutation.TryGetProperty("id", out _));
            Assert.True(mutation.TryGetProperty("type", out _));
            Assert.True(mutation.TryGetProperty("line", out _));
            Assert.True(mutation.TryGetProperty("original", out _));
            Assert.True(mutation.TryGetProperty("mutated", out _));
            Assert.True(mutation.TryGetProperty("testCaseCoverage", out _));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Report_ListsExactlyOneEntryPerMutation()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var reportJson = File.ReadAllText(Path.Combine(targetDirectory, "mutations-report.json"));
            using var document = JsonDocument.Parse(reportJson);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("totalMutations", out var total));
            Assert.Equal(package.Count, total.GetInt32());
            Assert.True(root.TryGetProperty("mutations", out var mutations));
            Assert.Equal(package.Count, mutations.EnumerateArray().Count());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Manifest_RecordsSourceCopiedAndCopybooksResolvedFlags()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            var root = document.RootElement;

            Assert.True(root.GetProperty("sourceCopied").GetBoolean());
            Assert.True(root.GetProperty("copybooksResolved").GetBoolean());
            Assert.Equal("medium", root.GetProperty("mutationProfile").GetString());
            Assert.Equal("PAYMENT", root.GetProperty("originalProgram").GetString());
            Assert.False(string.IsNullOrEmpty(root.GetProperty("baseProgramHash").GetString()));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_AttachesManifestAndReportToPackage()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            Assert.NotNull(package.Manifest);
            Assert.NotNull(package.Report);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_NullPackage_Throws()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);

        Assert.Throws<ArgumentNullException>(() => exporter.Export(null!, "out"));
    }

    [Fact]
    public void Exporter_DeclaresName()
    {
        Assert.Equal("mutant-package", new MutantPackageExporter().Name);
    }

    [Fact]
    public void Exporter_UsesProvidedEngine()
    {
        var program = new CobolProgram("PAYMENT", "IF A > B AND C = D");
        var package = new MutantPackage("PKG", program) { Profile = MutationProfile.Low };
        package.AddMutant(new Mutation("m1", MutationType.AndToOr, 1, "AND", "OR"));

        var exporter = new MutantPackageExporter(ExportFormat.Folder, new MutationEngine());
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var mutantFile = Directory.EnumerateFiles(targetDirectory, "*.cbl").Single(file =>
                !file.EndsWith("PAYMENT.cbl", StringComparison.Ordinal));
            Assert.Contains("IF A > B OR C = D", File.ReadAllText(mutantFile));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_PackageWithoutSourceProgram_HasEmptyBaseProgramHash()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = new MutantPackage("ALONE") { Profile = MutationProfile.Low };
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            Assert.Equal(string.Empty, document.RootElement.GetProperty("baseProgramHash").GetString());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Report_MutationIds_AreSequentialFromOne()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var reportJson = File.ReadAllText(Path.Combine(targetDirectory, "mutations-report.json"));
            using var document = JsonDocument.Parse(reportJson);
            var mutations = document.RootElement.GetProperty("mutations").EnumerateArray().ToList();
            Assert.Equal("MUT-001-PAYMENT-001", mutations[0].GetProperty("id").GetString());
            Assert.Equal("MUT-001-PAYMENT-002", mutations[1].GetProperty("id").GetString());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Manifest_UnknownMutationType_IsReportedAsUnknown()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = new MutantPackage("PAYMENT") { Profile = MutationProfile.High };
        package.AddMutant(new Mutation("m1", (MutationType)99, 1, "AND", "OR"));
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            var mutation = document.RootElement.GetProperty("mutations").EnumerateArray().First();
            Assert.Equal("unknown", mutation.GetProperty("type").GetString());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_BlankOutputDirectory_Throws()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();

        Assert.Throws<ArgumentException>(() => exporter.Export(package, " "));
    }

    [Fact]
    public void Export_PackageWithoutSourceProgram_UsesPackageId()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = new MutantPackage("ALONE") { Profile = MutationProfile.Low };
        package.AddMutant(new Mutation("m1", MutationType.AndToOr, 1, "AND", "OR"));
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            Assert.StartsWith("MUT-001-ALONE-001", Path.GetFileName(targetDirectory));
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            Assert.Equal("ALONE", document.RootElement.GetProperty("originalProgram").GetString());
            Assert.False(document.RootElement.GetProperty("sourceCopied").GetBoolean());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_ManifestAndReportUseProgramName()
    {
        var program = new CobolProgram("PAYMENT", "IF A > B AND C = D");
        var package = new MutantPackage("DIFFERENT", program) { Profile = MutationProfile.Low };
        package.AddMutant(new Mutation("m1", MutationType.AndToOr, 1, "AND", "OR"));
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var manifestDocument = JsonDocument.Parse(manifestJson);
            Assert.Equal("PAYMENT", manifestDocument.RootElement.GetProperty("originalProgram").GetString());

            var reportJson = File.ReadAllText(Path.Combine(targetDirectory, "mutations-report.json"));
            using var reportDocument = JsonDocument.Parse(reportJson);
            Assert.Equal("PAYMENT", reportDocument.RootElement.GetProperty("originalProgram").GetString());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_PackageWithoutCopybooks_ReportsCopybooksNotResolved()
    {
        var parser = new TypeCobolParserAdapter();
        var parseResult = parser.Parse(Source);
        var program = new CobolProgram("PAYMENT", Source, null, parseResult.Ast);
        var package = new MutantPackage("PAYMENT", program) { Profile = MutationProfile.Medium };
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            Assert.False(document.RootElement.GetProperty("copybooksResolved").GetBoolean());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_PackageWithoutMutants_WritesSourceAndEmptyReport()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = new MutantPackage("PAYMENT") { Profile = MutationProfile.High };
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var reportJson = File.ReadAllText(Path.Combine(targetDirectory, "mutations-report.json"));
            using var document = JsonDocument.Parse(reportJson);
            Assert.Equal(0, document.RootElement.GetProperty("totalMutations").GetInt32());
            Assert.Empty(document.RootElement.GetProperty("mutations").EnumerateArray());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void MutatedSource_AppliesMutationToLine()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var mutantFile = Directory.EnumerateFiles(targetDirectory, "MUT-001-PAYMENT-001.cbl").Single();
            var content = File.ReadAllText(mutantFile);
            Assert.Contains("IF A > B OR C = D", content);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ZipExport_ContainsMutantSourceFile()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Zip);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var zipPath = Directory.EnumerateFiles(outputDirectory, "*.zip").Single();
            using var archive = ZipFile.OpenRead(zipPath);
            Assert.Contains(archive.Entries, entry => entry.FullName == "MUT-001-PAYMENT-001.cbl");
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ZipExport_FileContentsAreNonEmpty()
    {
        var exporter = new MutantPackageExporter(ExportFormat.Zip);
        var package = CreateSamplePackage();
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var zipPath = Directory.EnumerateFiles(outputDirectory, "*.zip").Single();
            using var archive = ZipFile.OpenRead(zipPath);
            var contents = archive.Entries
                .Select(entry =>
                {
                    using var reader = new StreamReader(entry.Open());
                    return reader.ReadToEnd();
                })
                .ToList();
            Assert.All(contents, content => Assert.NotEmpty(content));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void Export_PackageIdDiffersFromProgramName_UsesProgramName()
    {
        var program = new CobolProgram("PAYMENT", "IF A > B AND C = D");
        var package = new MutantPackage("SOMETHING-ELSE", program) { Profile = MutationProfile.Low };
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            Assert.StartsWith("MUT-001-PAYMENT-001", Path.GetFileName(targetDirectory));
            Assert.True(File.Exists(Path.Combine(targetDirectory, "PAYMENT.cbl")));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(MutationType.AndToOr, "logical_operator")]
    [InlineData(MutationType.OrToAnd, "logical_operator")]
    [InlineData(MutationType.AddNot, "logical_operator")]
    [InlineData(MutationType.RemoveNot, "logical_operator")]
    [InlineData(MutationType.AddToSubtract, "arithmetic_operator")]
    [InlineData(MutationType.SubtractToAdd, "arithmetic_operator")]
    [InlineData(MutationType.MultiplyToDivide, "arithmetic_operator")]
    [InlineData(MutationType.DivideToMultiply, "arithmetic_operator")]
    [InlineData(MutationType.ConstantReplacement, "constant_replacement")]
    [InlineData(MutationType.ComplexExpressionMutation, "complex_expression")]
    public void Manifest_ReportsMutationTypeNames(MutationType type, string expectedName)
    {
        var exporter = new MutantPackageExporter(ExportFormat.Folder);
        var package = new MutantPackage("PAYMENT") { Profile = MutationProfile.High };
        package.AddMutant(new Mutation("m1", type, 1, "AND", "OR"));
        var outputDirectory = CreateTempDirectory();
        try
        {
            exporter.Export(package, outputDirectory);

            var targetDirectory = Directory.EnumerateDirectories(outputDirectory).Single();
            var manifestJson = File.ReadAllText(Path.Combine(targetDirectory, "manifest.json"));
            using var document = JsonDocument.Parse(manifestJson);
            var mutation = document.RootElement.GetProperty("mutations").EnumerateArray().First();
            Assert.Equal(expectedName, mutation.GetProperty("type").GetString());
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    private static MutantPackage CreateSamplePackage()
    {
        var parser = new TypeCobolParserAdapter();
        var parseResult = parser.Parse(Source);
        var program = new CobolProgram("PAYMENT", Source, new[] { "CUSTOMER" }, parseResult.Ast);

        var package = new MutantPackage("PAYMENT", program) { Profile = MutationProfile.Medium };
        package.AddMutant(new Mutation("MUT-PAYMENT-001", MutationType.AndToOr, 4, "AND", "OR", new[] { "TC-001" }));
        package.AddMutant(new Mutation("MUT-PAYMENT-002", MutationType.AddToSubtract, 5, "+", "-"));
        return package;
    }

    private static string CreateTempDirectory()
        => Path.Combine(Path.GetTempPath(), "cmf-export-" + Guid.NewGuid().ToString("N"));
}
