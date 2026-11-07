using System.Diagnostics;
using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

/// <summary>
/// Parsing the following Production:
/// Expression   := Term { Or Term }.
/// </summary>
internal class ExpressionParser : IParser
{
    // Break the dependency cycle
    public TermParser? TermParser { get; set; }

    public IParseNode Parse(ILexemeWalker walker)
    {
        Debug.Assert(TermParser != null, "The TermParser must be set before parsing, it is injected as a property to break the dependency cycle.");

        var result = TermParser.Parse(walker);

        while (walker.NextIs<OrToken>())
        {
            var lexem = walker.Pop();
            if (lexem.Token is OrToken)
            {
                result = new OrOperatorNode(result, TermParser.Parse(walker));
            }
        }

        return result;
    }
}