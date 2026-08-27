using CobolMutantForge.Domain.Ast;
using CobolMutantForge.Domain.Interfaces;

namespace CobolMutantForge.Infrastructure.Parsers;

/// <summary>
/// Adapter implementing <see cref="ICobolParser"/>.
///
/// The PRD mandates TypeCobol as the COBOL parser. As of this change there is no
/// consumable TypeCobol package on NuGet (the compiler core is distributed only as
/// source in TypeCobolTeam/TypeCobol). This adapter therefore runs a self-contained
/// fallback parser that satisfies the MVP contract: it discovers logical operators
/// (AND/OR/NOT) and arithmetic operators (+ - * /) outside comments and string
/// literals, and reports diagnostics with line/column information. When a consumable
/// TypeCobol artifact becomes available, this adapter should be rewired to map
/// TypeCobol's parse tree onto <see cref="AstNode"/> without changing callers.
/// </summary>
public sealed class TypeCobolParserAdapter : ICobolParser
{
    private static readonly HashSet<string> LogicalOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "OR", "NOT"
    };

    private static readonly HashSet<string> KnownStatementKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "ACCEPT", "ADD", "AUTHOR", "CALL", "CANCEL", "CLOSE", "COMPUTE", "CONTINUE",
        "DATA", "DECLARATIVES", "DELETE", "DISPLAY", "DIVIDE", "DIVISION",
        "ELSE", "END-CALL", "END-DECLARATIVES", "END-EVALUATE", "END-IF",
        "END-PERFORM", "END-PROGRAM", "END-STRING", "END-SUBTRACT", "END-ADD",
        "END-MULTIPLY", "END-DIVIDE", "END-READ", "END-WRITE", "ENVIRONMENT",
        "EVALUATE", "EXIT", "GO", "GOTO", "IDENTIFICATION", "IF", "INITIALIZE",
        "INSPECT", "LINKAGE", "MOVE", "MULTIPLY", "OPEN", "PERFORM", "PROCEDURE",
        "PROGRAM-ID", "READ", "SECTION", "SET", "STOP", "STRING", "SUBTRACT",
        "UNSTRING", "WHEN", "WORKING-STORAGE", "WRITE"
    };

    public ParseResult Parse(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);

        var nodes = new List<AstNode>();
        var diagnostics = new List<ParseDiagnostic>();

        var lines = sourceText.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.EndsWith("\r"))
            {
                line = line[..^1];
            }

            ScanLine(line, index + 1, nodes, diagnostics);
        }

        var ast = new AstNode
        {
            Kind = "Program",
            Line = 1,
            Column = 1,
            Children = nodes
        };

        return new ParseResult
        {
            Ast = ast,
            Diagnostics = diagnostics
        };
    }

    private static void ScanLine(string line, int lineNumber, List<AstNode> nodes, List<ParseDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        // Comment lines: '*' in the indicator column (fixed format) or '*'/'*/' at the start.
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith('*'))
        {
            return;
        }

        // Strip inline comments ('*>...').
        int inlineComment = line.IndexOf("*>", StringComparison.Ordinal);
        if (inlineComment >= 0)
        {
            line = line.Substring(0, inlineComment);
        }

        WarnOnUnsupportedConstruct(line, lineNumber, diagnostics);

        char? stringQuote = null;
        int stringStartColumn = -1;
        int wordStart = -1;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (stringQuote.HasValue)
            {
                if (c == stringQuote.Value)
                {
                    stringQuote = null;
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                stringQuote = c;
                stringStartColumn = i + 1;
                continue;
            }

            if (c == '-')
            {
                bool isHyphenInWord = i > 0 && i < line.Length - 1 &&
                    char.IsLetterOrDigit(line[i - 1]) && char.IsLetterOrDigit(line[i + 1]);
                if (!isHyphenInWord)
                {
                    if (wordStart >= 0)
                    {
                        EmitWord(line, wordStart, i, lineNumber, nodes);
                        wordStart = -1;
                    }
                    nodes.Add(new AstNode
                    {
                        Kind = "ArithmeticOperator",
                        Line = lineNumber,
                        Column = i + 1,
                        Text = "-"
                    });
                    continue;
                }
            }

            if (char.IsLetterOrDigit(c) || c == '-')
            {
                if (wordStart < 0)
                {
                    wordStart = i;
                }
                continue;
            }

            if (wordStart >= 0)
            {
                EmitWord(line, wordStart, i, lineNumber, nodes);
                wordStart = -1;
            }

            if (c == '+' || c == '*' || c == '/')
            {
                nodes.Add(new AstNode
                {
                    Kind = "ArithmeticOperator",
                    Line = lineNumber,
                    Column = i + 1,
                    Text = c.ToString()
                });
            }
        }

        if (stringQuote.HasValue)
        {
            diagnostics.Add(new ParseDiagnostic(
                DiagnosticSeverity.Error,
                $"Unterminated string literal on line {lineNumber}.",
                lineNumber,
                stringStartColumn));
        }

        if (wordStart >= 0)
        {
            EmitWord(line, wordStart, line.Length, lineNumber, nodes);
        }
    }

    private static void EmitWord(string line, int start, int end, int lineNumber, List<AstNode> nodes)
    {
        var word = line.Substring(start, end - start);
        var upper = word.ToUpperInvariant();
        if (LogicalOperators.Contains(upper))
        {
            nodes.Add(new AstNode
            {
                Kind = "LogicalOperator",
                Line = lineNumber,
                Column = start + 1,
                Text = upper
            });
        }
    }

    private static void WarnOnUnsupportedConstruct(string line, int lineNumber, List<ParseDiagnostic> diagnostics)
    {
        var match = System.Text.RegularExpressions.Regex.Match(line.TrimStart(), @"^[A-Za-z0-9][A-Za-z0-9-]*");
        if (!match.Success)
        {
            return;
        }

        var token = match.Value;
        if (KnownStatementKeywords.Contains(token))
        {
            return;
        }

        if (char.IsDigit(token[0]) || token.EndsWith('.'))
        {
            return;
        }

        diagnostics.Add(new ParseDiagnostic(
            DiagnosticSeverity.Warning,
            $"Possibly unsupported construct '{token}' on line {lineNumber}; the line was scanned heuristically.",
            lineNumber,
            line.IndexOf(token, StringComparison.Ordinal) + 1));
    }
}
