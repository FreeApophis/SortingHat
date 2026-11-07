using Funcky.Lexer;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser;

internal interface IParser
{
    IParseNode Parse(ILexemeWalker walker);
}
