using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

/// <summary>
/// Parsing the following Production:
/// Term       := Factor { And Factor }.
/// </summary>
public class TermParser(FactorParser factorParser) : IParser
{
    /// <summary>
    /// Overloaded Parse function to parse a Term.
    /// </summary>
    /// <param name="walker">Lexer input.</param>
    /// <returns>The parsed term.</returns>
    public IParseNode Parse(ILexemeWalker walker)
    {
        var result = factorParser.Parse(walker);
        while (walker.NextIs<AndToken>())
        {
            var lexem = walker.Pop();
            if (lexem.Token is AndToken)
            {
                result = new AndOperatorNode(result, factorParser.Parse(walker));
            }
        }

        return result;
    }
}