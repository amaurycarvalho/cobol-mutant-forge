using CobolMutantForge.Domain.Ast;

namespace CobolMutantForge.Domain.Interfaces;

public interface ICobolParser
{
    ParseResult Parse(string sourceText);
}
