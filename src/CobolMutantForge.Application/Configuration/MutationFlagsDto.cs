namespace CobolMutantForge.Application.Configuration;

public sealed class MutationFlagsDto
{
    public bool LogicalOperators { get; set; }
    public bool ArithmeticOperators { get; set; }
    public bool ComplexExpressions { get; set; }
    public bool NumericConstants { get; set; }
    public bool StringConstants { get; set; }
}
