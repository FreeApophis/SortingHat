using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

public class FactorParser(ExpressionParser expressionParser) : IParser
{
    /// <summary>
    /// Parsing the following Production:
    /// Factor       := Tag | BoolConstant | Not Factor | "(" Expression ").
    /// </summary>
    public IParseNode Parse(ILexemeWalker walker)
    {
        if (walker.NextIs<TagToken>())
        {
            var lexem = walker.Pop();
            if (lexem.Token is TagToken tagToken && tagToken.Value is { } value)
            {
                return new TagNode(value);
            }
        }

        if (walker.NextIs<BoolConstantToken>())
        {
            var lexem = walker.Pop();
            if (lexem.Token is BoolConstantToken boolToken)
            {
                return new BooleanNode(boolToken.Value);
            }
        }

        if (walker.NextIs<NotToken>())
        {
            walker.Pop();
            return new NotOperatorNode(Parse(walker));
        }

        walker.ExpectOpeningParenthesis();
        var result = expressionParser.Parse(walker);
        walker.ExpectClosingParenthesis();

        return result;
    }
}