using apophis.Lexer;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{
    public interface IParser
    {
        IParseNode Parse(TokenWalker walker);
    }
}
