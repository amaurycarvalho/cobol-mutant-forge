namespace CobolMutantForge.Application.Configuration;

public static class DefaultConfigFactory
{
    public static MutationConfigDto CreateDefault()
    {
        return new MutationConfigDto
        {
            ProjectName = "CobolMutantForge",
            Version = "0.1.0",
            Paths = new PathsDto
            {
                SourceDirectory = "src",
                TestDataDirectory = "tests",
                OutputDirectory = "output",
                CopybookDirectory = "copybooks"
            },
            MutationProfile = MutationProfile.Medium,
            MutationFlags = new MutationFlagsDto
            {
                LogicalOperators = true,
                ArithmeticOperators = true,
                ComplexExpressions = false,
                NumericConstants = true,
                StringConstants = false
            },
            Zunit = new Dictionary<string, object?>(),
            TestAccelerator = new Dictionary<string, object?>(),
            Export = new Dictionary<string, object?>()
        };
    }
}
