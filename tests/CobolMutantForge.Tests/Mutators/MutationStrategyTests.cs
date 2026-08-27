using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.Mutators;

public class MutationStrategyTests
{
    private const string NotProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF NOT A > B\r\n" +
        "               DISPLAY A\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    [Fact]
    public void Logical_And_ProducesAndToOr()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.AndToOr
            && mutation.Original == "AND"
            && mutation.Mutated == "OR");
    }

    [Fact]
    public void Logical_Or_ProducesOrToAnd()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B OR C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.OrToAnd
            && mutation.Original == "OR"
            && mutation.Mutated == "AND");
    }

    [Fact]
    public void Logical_Not_ProducesRemoveNot()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(NotProgram);

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.RemoveNot
            && mutation.Original == "NOT"
            && mutation.Mutated == string.Empty);
    }

    [Fact]
    public void Logical_IfCondition_ProducesAddNot()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation =>
            mutation.Type == MutationType.AddNot
            && mutation.Original == "IF "
            && mutation.Mutated == "IF NOT ");
    }

    [Fact]
    public void Logical_AlreadyNegatedCondition_DoesNotAddNot()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF NOT A > B AND C > D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.DoesNotContain(mutations, mutation => mutation.Type == MutationType.AddNot);
    }

    [Theory]
    [InlineData("+", "-", MutationType.AddToSubtract)]
    [InlineData("-", "+", MutationType.SubtractToAdd)]
    [InlineData("*", "/", MutationType.MultiplyToDivide)]
    [InlineData("/", "*", MutationType.DivideToMultiply)]
    public void Arithmetic_Operator_ProducesCounterpart(
        string original, string mutated, MutationType expectedType)
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            $"           COMPUTE TOTAL = AMOUNT {original} TAX\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation =>
            mutation.Type == expectedType
            && mutation.Original == original
            && mutation.Mutated == mutated);
    }

    [Fact]
    public void ConstantStrategy_IsInert()
    {
        var strategy = new ConstantMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE 1 TO A\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
        Assert.Equal(MutationType.ConstantReplacement, strategy.MutationType);
    }

    [Fact]
    public void ComplexExpressionStrategy_IsInert()
    {
        var strategy = new ComplexExpressionMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           EVALUATE TRUE\r\n" +
            "           END-EVALUATE.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
        Assert.Equal(MutationType.ComplexExpressionMutation, strategy.MutationType);
    }

    [Fact]
    public void ConstantStrategy_MutationType_IsConstantReplacement()
    {
        Assert.Equal(MutationType.ConstantReplacement, new ConstantMutationStrategy().MutationType);
    }

    [Fact]
    public void ComplexExpressionStrategy_MutationType_IsComplexExpressionMutation()
    {
        Assert.Equal(MutationType.ComplexExpressionMutation, new ComplexExpressionMutationStrategy().MutationType);
    }

    [Fact]
    public void ConstantStrategy_IsInertForProgramWithAst()
    {
        var strategy = new ConstantMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE 1 TO A\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void ComplexExpressionStrategy_IsInertForProgramWithAst()
    {
        var strategy = new ComplexExpressionMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           EVALUATE TRUE\r\n" +
            "           END-EVALUATE.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void Logical_DeeplyNestedAst_FindsOperators()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var ast = new AstNode
        {
            Kind = "Program",
            Children = new[]
            {
                new AstNode
                {
                    Kind = "Statement",
                    Children = new[]
                    {
                        new AstNode { Kind = "LogicalOperator", Text = "AND", Line = 1 }
                    }
                }
            }
        };
        var program = new CobolProgram("SAMPLE", "IF A > B AND C = D", null, ast);

        var mutations = strategy.Apply(program);

        Assert.Single(mutations, mutation => mutation.Type == MutationType.AndToOr);
    }

    [Fact]
    public void Arithmetic_DeeplyNestedAst_FindsOperators()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var ast = new AstNode
        {
            Kind = "Program",
            Children = new[]
            {
                new AstNode
                {
                    Kind = "Statement",
                    Children = new[]
                    {
                        new AstNode { Kind = "ArithmeticOperator", Text = "+", Line = 1 }
                    }
                }
            }
        };
        var program = new CobolProgram("SAMPLE", "COMPUTE TOTAL = AMOUNT + TAX", null, ast);

        var mutations = strategy.Apply(program);

        Assert.Single(mutations, mutation => mutation.Type == MutationType.AddToSubtract);
    }

    [Fact]
    public void Strategy_DeclaresOperationTypeForProfileGating()
    {
        Assert.Equal(OperationType.Logical, new LogicalOperatorMutationStrategy().OperationType);
        Assert.Equal(OperationType.Arithmetic, new ArithmeticOperatorMutationStrategy().OperationType);
        Assert.Equal(OperationType.Constant, new ConstantMutationStrategy().OperationType);
        Assert.Equal(OperationType.ComplexExpression, new ComplexExpressionMutationStrategy().OperationType);
    }

    [Fact]
    public void Logical_NullProgram_Throws()
    {
        var strategy = new LogicalOperatorMutationStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.Apply(null!));
    }

    [Fact]
    public void Logical_ProgramWithoutAst_ProducesNoMutations()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = new CobolProgram("SAMPLE", NotProgram);

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void Logical_MutationType_IsAndToOr()
    {
        Assert.Equal(MutationType.AndToOr, new LogicalOperatorMutationStrategy().MutationType);
    }

    [Fact]
    public void Logical_NotOnNonIfLine_DoesNotInsertNot()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B AND C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation => mutation.Type == MutationType.AndToOr);
        Assert.DoesNotContain(mutations, mutation => mutation.Type == MutationType.AddNot);
    }

    [Fact]
    public void Logical_NonLogicalNodes_AreIgnored()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B + C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void Logical_OrOnIfLine_ProducesOrToAndAndAddNot()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B OR C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Single(mutations, mutation => mutation.Type == MutationType.OrToAnd);
        Assert.Single(mutations, mutation => mutation.Type == MutationType.AddNot);
    }

    [Fact]
    public void Arithmetic_NullProgram_Throws()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.Apply(null!));
    }

    [Fact]
    public void Arithmetic_ProgramWithoutAst_ProducesNoMutations()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = new CobolProgram("SAMPLE", "COMPUTE A = B + C");

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void Arithmetic_MutationType_IsAddToSubtract()
    {
        Assert.Equal(MutationType.AddToSubtract, new ArithmeticOperatorMutationStrategy().MutationType);
    }

    [Fact]
    public void Arithmetic_NonArithmeticNodes_AreIgnored()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        Assert.Empty(strategy.Apply(program));
    }

    [Fact]
    public void Arithmetic_MultipleOperators_ProduceOneMutationPerOperator()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE TOTAL = A + B + C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Equal(2, mutations.Count);
        Assert.All(mutations, mutation =>
        {
            Assert.Equal(MutationType.AddToSubtract, mutation.Type);
            Assert.Equal("+", mutation.Original);
            Assert.Equal("-", mutation.Mutated);
        });
    }

    [Fact]
    public void Arithmetic_MultipleOperators_AssignSequentialTemporaryIds()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE TOTAL = A + B + C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Equal(new[] { "tmp-0", "tmp-1" }, mutations.Select(mutation => mutation.Id));
    }

    [Fact]
    public void Arithmetic_MixedOperators_AssignSequentialTemporaryIds()
    {
        var strategy = new ArithmeticOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE TOTAL = A - B * C / D\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Equal(new[] { "tmp-0", "tmp-1", "tmp-2" }, mutations.Select(mutation => mutation.Id));
        Assert.Equal(new[]
        {
            MutationType.SubtractToAdd,
            MutationType.MultiplyToDivide,
            MutationType.DivideToMultiply
        }, mutations.Select(mutation => mutation.Type));
    }

    [Fact]
    public void Logical_MultipleOperators_AssignSequentialTemporaryIds()
    {
        var strategy = new LogicalOperatorMutationStrategy();
        var program = CreateProgram(
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A AND B AND C\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n");

        var mutations = strategy.Apply(program);

        Assert.Contains(mutations, mutation => mutation.Type == MutationType.AndToOr && mutation.Id == "tmp-0");
        Assert.Contains(mutations, mutation => mutation.Type == MutationType.AndToOr && mutation.Id == "tmp-1");
    }

    private static CobolProgram CreateProgram(string source)
    {
        var parser = new TypeCobolParserAdapter();
        var result = parser.Parse(source);
        return new CobolProgram("SAMPLE", source, null, result.Ast);
    }
}
