using Funcky.Lexer.Token;

namespace SortingHat.API.Parser.Token;

public class EpsilonToken : IEpsilonToken
{
    public override string ToString() => "ε : End of expression";
}
