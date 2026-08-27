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
        var andNode = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "LogicalOperator" && n.Text == "AND");
        Assert.Equal(8, andNode.Line);
        Assert.True(andNode.Column > 0);
    }

    [Fact]
    public void Parse_ProgramNode_HasExpectedKindLineAndColumn()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse(ComputeProgram);

        Assert.Equal("Program", result.Ast.Kind);
        Assert.Equal(1, result.Ast.Line);
        Assert.Equal(1, result.Ast.Column);
    }

    [Fact]
    public void Parse_LowercaseAnd_IsRecognizedCaseInsensitively()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B and C = D\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var andNode = Assert.Single(AllNodes(result.Ast), n => n.Kind == "LogicalOperator");
        Assert.Equal("AND", andNode.Text);
        Assert.Equal(4, andNode.Line);
    }

    [Fact]
    public void Parse_OrAndNot_ProduceDistinctLogicalNodesWithLocations()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF NOT A OR B AND C\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var logicalNodes = AllNodes(result.Ast)
            .Where(n => n.Kind == "LogicalOperator")
            .ToList();
        Assert.Equal(3, logicalNodes.Count);
        Assert.Equal(new[] { "NOT", "OR", "AND" }, logicalNodes.Select(n => n.Text));
        Assert.All(logicalNodes, node => Assert.Equal(4, node.Line));
        Assert.Equal(15, logicalNodes[0].Column);
        Assert.Equal(21, logicalNodes[1].Column);
        Assert.Equal(26, logicalNodes[2].Column);
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
        var plusNode = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "+");
        Assert.Equal(4, plusNode.Line);
        Assert.True(plusNode.Column > 0);
    }

    [Fact]
    public void Parse_MultipleArithmeticOperators_ReportsExactTextAndColumns()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE TOTAL = A + B - C * D / E\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var operators = AllNodes(result.Ast)
            .Where(n => n.Kind == "ArithmeticOperator")
            .ToList();
        Assert.Equal(4, operators.Count);
        Assert.Equal(new[] { "+", "-", "*", "/" }, operators.Select(n => n.Text));
        Assert.All(operators, node => Assert.Equal(4, node.Line));
        Assert.Equal(new[] { 30, 34, 38, 42 }, operators.Select(n => n.Column));
    }

    [Fact]
    public void Parse_InlineComment_OperatorsAfterMarkerAreIgnored()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE TOTAL = A + B *> NOT A REAL OPERATOR\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var operators = AllNodes(result.Ast)
            .Where(n => n.Kind == "ArithmeticOperator")
            .ToList();
        Assert.Single(operators);
        Assert.Equal("+", operators[0].Text);
    }

    [Fact]
    public void Parse_LineStartingWithInlineComment_IsIgnored()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "      *> AND OR NOT + - * /\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast), n => n.Kind != "Program");
    }

    [Fact]
    public void Parse_ClosedStringFollowedByOperator_DiscoversOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE 'X' TO A AND B\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.Contains(AllNodes(result.Ast), n => n.Kind == "LogicalOperator" && n.Text == "AND");
    }

    [Fact]
    public void Parse_CommentLineInIndicatorColumn_IsIgnored()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "      * IF A > B AND C = D\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast), n => n.Kind != "Program");
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Parse_BlankAndWhitespaceLines_AreIgnored()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "\r\n" +
            "       \r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast), n => n.Kind != "Program");
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Parse_DoubleQuotedString_OperatorsInsideAreIgnored()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE \"A + B AND C\" TO TEXT\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast), n => n.Kind != "Program");
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
        Assert.Equal("Unterminated string literal on line 4.", error.Message);
        Assert.True(error.Column > 0);
        Assert.Equal(17, error.Column);
    }

    [Fact]
    public void Parse_UnterminatedDoubleQuotedString_ReportsErrorWithExactMessage()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE \"oops TO A\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.True(result.HasErrors);
        var error = Assert.Single(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(4, error.Line);
        Assert.Equal("Unterminated string literal on line 4.", error.Message);
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
        var warning = Assert.Single(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Warning);
        Assert.Equal(
            "Possibly unsupported construct 'FANCY-STATEMENT' on line 4; the line was scanned heuristically.",
            warning.Message);
        Assert.Equal(4, warning.Line);
        Assert.Equal(12, warning.Column);
    }

    [Fact]
    public void Parse_LineStartingWithMinus_EmitsOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "- FIRST\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        var minus = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
        Assert.Equal(4, minus.Line);
        Assert.Equal(1, minus.Column);
    }

    [Fact]
    public void Parse_LineEndingWithMinus_EmitsOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B -\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        var minus = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
        Assert.Equal(4, minus.Line);
    }

    [Fact]
    public void Parse_MinusBeforeIdentifier_IsTreatedAsOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = -B\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        var minus = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
        Assert.Equal(4, minus.Line);
    }

    [Fact]
    public void Parse_MinusAfterIdentifierWithTrailingSpace_IsTreatedAsOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B- C\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        var minus = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
        Assert.Equal(4, minus.Line);
    }

    [Fact]
    public void Parse_WordAtColumnZero_IsRecognizedAsLogicalOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "AND ALONE\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var andNode = Assert.Single(AllNodes(result.Ast), n => n.Kind == "LogicalOperator");
        Assert.Equal("AND", andNode.Text);
        Assert.Equal(1, andNode.Column);
    }

    [Fact]
    public void Parse_SingleWordAtColumnZero_IsRecognizedAtLineEnd()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "AND\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var andNode = Assert.Single(AllNodes(result.Ast), n => n.Kind == "LogicalOperator");
        Assert.Equal("AND", andNode.Text);
        Assert.Equal(1, andNode.Column);
    }

    [Fact]
    public void Parse_WordAtColumnZeroBeforeMinus_IsRecognized()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "AND -X\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.Contains(AllNodes(result.Ast), n => n.Kind == "LogicalOperator" && n.Text == "AND");
    }

    [Fact]
    public void Parse_LineEndingWithHyphenAfterLetter_EmitsOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           COMPUTE A = B-\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.Contains(AllNodes(result.Ast), n => n.Kind == "ArithmeticOperator" && n.Text == "-");
    }

    [Fact]
    public void Parse_KnownStatements_ProduceNoWarnings()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       DATA DIVISION.\r\n" +
            "       WORKING-STORAGE SECTION.\r\n" +
            "       01 A PIC 9 VALUE 0.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE 1 TO A\r\n" +
            "           PERFORM VARYING I FROM 1 BY 1 UNTIL I > 10\r\n" +
            "               DISPLAY A\r\n" +
            "           END-PERFORM.\r\n" +
            "           STOP RUN.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Parse_NumericAndPeriodTerminatedTokens_ProduceNoWarnings()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "       01 LEVEL-NUM PIC 9.\r\n" +
            "           GOTO 100-PARA.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.False(result.HasErrors);
        Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Warning);
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
        var minusNode = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "ArithmeticOperator" && n.Text == "-");
        Assert.Equal(4, minusNode.Line);
        Assert.Equal(26, minusNode.Column);
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
            n => n.Kind == "ArithmeticOperator" || n.Kind == "LogicalOperator");
    }

    [Fact]
    public void Parse_HyphenatedIdentifierAtLineEnd_DoesNotProduceMinusNode()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           MOVE CUSTOMER-ACTIVE\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        Assert.DoesNotContain(AllNodes(result.Ast), n => n.Kind != "Program");
    }

    [Fact]
    public void Parse_WordEndingAtLineEnd_DoesNotProduceOperator()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\r\n" +
            "           IF A > B AND\r\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\r\n";

        var result = parser.Parse(source);

        var andNode = Assert.Single(AllNodes(result.Ast), n => n.Kind == "LogicalOperator");
        Assert.Equal("AND", andNode.Text);
        Assert.Equal(4, andNode.Line);
    }

    [Fact]
    public void Parse_EmptySource_ProducesProgramNodeWithoutErrors()
    {
        var parser = new TypeCobolParserAdapter();

        var result = parser.Parse("");

        Assert.Equal("Program", result.Ast.Kind);
        Assert.Equal(1, result.Ast.Line);
        Assert.Equal(1, result.Ast.Column);
        Assert.Empty(result.Ast.Children);
        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Parse_NullSource_ThrowsArgumentNullException()
    {
        var parser = new TypeCobolParserAdapter();

        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }

    [Fact]
    public void Parse_LineWithoutTrailingCr_CrLfIsNormalized()
    {
        var parser = new TypeCobolParserAdapter();
        const string source =
            "       IDENTIFICATION DIVISION.\r\n" +
            "       PROGRAM-ID. SAMPLE.\r\n" +
            "       PROCEDURE DIVISION.\n" +
            "           IF A > B AND C = D\n" +
            "           END-IF.\r\n" +
            "       END-PROGRAM SAMPLE.\n";

        var result = parser.Parse(source);

        var andNode = Assert.Single(AllNodes(result.Ast),
            n => n.Kind == "LogicalOperator" && n.Text == "AND");
        Assert.Equal(4, andNode.Line);
    }
}
