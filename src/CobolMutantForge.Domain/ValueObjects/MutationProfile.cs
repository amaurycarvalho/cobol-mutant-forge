namespace CobolMutantForge.Domain.ValueObjects;

public sealed record MutationProfile
{
    public string Name { get; }
    public bool LogicalOperators { get; }
    public bool ArithmeticOperators { get; }
    public bool ComplexExpressions { get; }
    public bool NumericConstants { get; }
    public bool StringConstants { get; }

    public static readonly MutationProfile Low = new(
        "low",
        logicalOperators: true,
        arithmeticOperators: false,
        complexExpressions: false,
        numericConstants: false,
        stringConstants: false);

    public static readonly MutationProfile Medium = new(
        "medium",
        logicalOperators: true,
        arithmeticOperators: true,
        complexExpressions: false,
        numericConstants: true,
        stringConstants: false);

    public static readonly MutationProfile High = new(
        "high",
        logicalOperators: true,
        arithmeticOperators: true,
        complexExpressions: true,
        numericConstants: true,
        stringConstants: true);

    private MutationProfile(
        string name,
        bool logicalOperators,
        bool arithmeticOperators,
        bool complexExpressions,
        bool numericConstants,
        bool stringConstants)
    {
        Name = name;
        LogicalOperators = logicalOperators;
        ArithmeticOperators = arithmeticOperators;
        ComplexExpressions = complexExpressions;
        NumericConstants = numericConstants;
        StringConstants = stringConstants;
    }

    public static MutationProfile FromName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return name.Trim().ToLowerInvariant() switch
        {
            "low" => Low,
            "medium" => Medium,
            "high" => High,
            _ => throw new ArgumentException(
                $"Unknown mutation profile '{name}'. Expected one of: low, medium, high.",
                nameof(name))
        };
    }
}
