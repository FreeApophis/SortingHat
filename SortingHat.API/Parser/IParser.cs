using Funcky.Lexer;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser;

public interface IParser
{
    IParseNode Parse(ILexemeWalker walker);
}
