namespace CobolMutantForge.Domain.ValueObjects;

public enum MutationType
{
    AndToOr,
    OrToAnd,
    AddNot,
    RemoveNot,
    AddToSubtract,
    SubtractToAdd,
    MultiplyToDivide,
    DivideToMultiply,
    ConstantReplacement,
    ComplexExpressionMutation
}
