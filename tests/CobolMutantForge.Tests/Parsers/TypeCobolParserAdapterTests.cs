using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Infrastructure.Parsers;
using Xunit;

namespace CobolMutantForge.Tests.Parsers;

public class TypeCobolParserAdapterTests
{
    private const string IfProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       DATA DIVISION.\r\n" +
        "       WORKING-STORAGE SECTION.\r\n" +
        "       01 A PIC 9 VALUE 0.\r\n" +
        "       01 B PIC 9 VALUE 1.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > 0 AND CUSTOMER-ACTIVE\r\n" +
        "               MOVE 1 TO A\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    private const string ComputeProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    private const string CommentedOperatorsProgram =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. SAMPLE.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "      * AND OR NOT + - * /\r\n" +
        "           MOVE 'AND OR +' TO TEXT\r\n" +
        "           COMPUTE A = B + C\r\n" +
        "       END-PROGRAM SAMPLE.\r\n";

    private static IEnumerable<AstNode> AllNodes(AstNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in AllNodes(child))
            {
                yield return descendant;
            }
        }
    }

    [Fact]
    public void Parse_IfAndProgram_DiscoversAndNode()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse(IfProgram);

        Assert.False(result.HasErrors);
        var andNodes = AllNodes(result.Ast)
            .Where(n => n.Kind == "LogicalOperator" && n.Text == "AND");
        Assert.NotEmpty(andNodes);
    }

    [Fact]
    public void Parse_IfAndProgram_DoesNotReportFatalErrors()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse(IfProgram);

        Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_ComputePlusProgram_DiscoversPlusNode()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse(ComputeProgram);

        Assert.False(result.HasErrors);
        var plusNodes = AllNodes(result.Ast)
            .Where(n => n.Kind == "ArithmeticOperator" && n.Text == "+");
        Assert.NotEmpty(plusNodes);
    }

    [Fact]
    public void Parse_CommentedOperators_AreExcluded()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse(CommentedOperatorsProgram);

        var operatorNodes = AllNodes(result.Ast)
            .Where(n => n.Kind == "LogicalOperator" || n.Kind == "ArithmeticOperator")
            .ToList();

        // Comment line and string literal tokens must not surface; only B + C's '+' remains.
        Assert.Single(operatorNodes);
        var node = Assert.Single(operatorNodes);
        Assert.Equal("+", node.Text);
        Assert.Equal(6, node.Line);
    }

    [Fact]
    public void Parse_UnterminatedString_ReportsErrorWithLocation()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE 'unterminated TO A\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.True(result.HasErrors);
        var error = Assert.Single(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(4, error.Line);
        Assert.True(error.Column > 0);
    }

    [Fact]
    public void Parse_UnsupportedConstruct_ReportsWarningNotError()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           FANCY-STATEMENT 1 TO 2\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Parse_StandaloneMinus_DiscoversMinusNode()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B - C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        Assert.Contains(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
    }

    [Fact]
    public void Parse_HyphenatedIdentifier_DoesNotProduceMinusNode()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE CUSTOMER-ACTIVE TO FLAG\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
    }

    [Fact]
    public void Parse_EmptySource_ProducesProgramNodeWithoutErrors()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse("");

        Assert.Equal("Program", result.Ast.Kind);
        Assert.False(result.HasErrors);
    }
}
