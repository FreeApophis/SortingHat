using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

/// <summary>
/// Parsing the following Production:
/// Term       := Factor { And Factor }.
/// </summary>
public class TermParser : IParser
{
    private readonly FactorParser _factorParser;

    public TermParser(FactorParser factorParser)
    {
        _factorParser = factorParser;
    }

    /// <summary>
    /// Overloaded Parse function to parse a Term.
    /// </summary>
    /// <param name="walker">Lexer input.</param>
    /// <returns>The parsed term.</returns>
    public IParseNode Parse(ILexemeWalker walker)
    {
        var result = _factorParser.Parse(walker);
        while (walker.NextIs<AndToken>())
        {
            var lexem = walker.Pop();
            if (lexem.Token is AndToken)
            {
                result = new AndOperatorNode(result, _factorParser.Parse(walker));
            }
        }

        return result;
    }
}