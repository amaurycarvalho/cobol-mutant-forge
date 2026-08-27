using CobolMutantForge.Infrastructure.Exporters;
using Xunit;

namespace CobolMutantForge.Tests.Exporters;

public class ExportDtosTests
{
    [Fact]
    public void MutationEntryDto_DefaultsAreEmpty()
    {
        var dto = new MutationEntryDto();

        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Type);
        Assert.Equal(0, dto.Line);
        Assert.Equal(string.Empty, dto.Original);
        Assert.Equal(string.Empty, dto.Mutated);
        Assert.Empty(dto.TestCaseCoverage);
    }

    [Fact]
    public void MutationEntryDto_ExposesSetValues()
    {
        var dto = new MutationEntryDto
        {
            Id = "id",
            Type = "logical_operator",
            Line = 4,
            Original = "AND",
            Mutated = "OR",
            TestCaseCoverage = new[] { "TC-1" }
        };

        Assert.Equal("id", dto.Id);
        Assert.Equal("logical_operator", dto.Type);
        Assert.Equal(4, dto.Line);
        Assert.Equal("AND", dto.Original);
        Assert.Equal("OR", dto.Mutated);
        Assert.Equal(new[] { "TC-1" }, dto.TestCaseCoverage);
    }

    [Fact]
    public void ManifestDto_DefaultsAreEmpty()
    {
        var dto = new ManifestDto();

        Assert.Equal(string.Empty, dto.MutantId);
        Assert.Equal(string.Empty, dto.OriginalProgram);
        Assert.Equal(string.Empty, dto.BaseProgramHash);
        Assert.Equal(string.Empty, dto.MutationProfile);
        Assert.Empty(dto.Mutations);
        Assert.False(dto.SourceCopied);
        Assert.False(dto.CopybooksResolved);
    }

    [Fact]
    public void ManifestDto_ExposesSetValues()
    {
        var dto = new ManifestDto
        {
            MutantId = "MUT-1",
            OriginalProgram = "PAYMENT",
            BaseProgramHash = "ABC",
            MutationProfile = "high",
            Mutations = new[] { new MutationEntryDto() },
            SourceCopied = true,
            CopybooksResolved = true
        };

        Assert.Equal("MUT-1", dto.MutantId);
        Assert.Equal("PAYMENT", dto.OriginalProgram);
        Assert.Equal("ABC", dto.BaseProgramHash);
        Assert.Equal("high", dto.MutationProfile);
        Assert.Single(dto.Mutations);
        Assert.True(dto.SourceCopied);
        Assert.True(dto.CopybooksResolved);
    }

    [Fact]
    public void MutationsReportDto_DefaultsAreEmpty()
    {
        var dto = new MutationsReportDto();

        Assert.Equal(string.Empty, dto.MutantId);
        Assert.Equal(string.Empty, dto.OriginalProgram);
        Assert.Equal(0, dto.TotalMutations);
        Assert.Empty(dto.Mutations);
    }

    [Fact]
    public void MutationsReportDto_ExposesSetValues()
    {
        var dto = new MutationsReportDto
        {
            MutantId = "MUT-1",
            OriginalProgram = "PAYMENT",
            TotalMutations = 3,
            Mutations = new[] { new MutationEntryDto(), new MutationEntryDto(), new MutationEntryDto() }
        };

        Assert.Equal("MUT-1", dto.MutantId);
        Assert.Equal("PAYMENT", dto.OriginalProgram);
        Assert.Equal(3, dto.TotalMutations);
        Assert.Equal(3, dto.Mutations.Count);
    }
}
