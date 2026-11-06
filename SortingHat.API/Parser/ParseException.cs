using System;
using Funcky.Lexer.Token;

namespace SortingHat.API.Parser;

public class ParseException : Exception
{
    public ParseException(string message)
        : base(message)
    {
    }
}

public class ExpectedTokenException : Exception
{
    public ExpectedTokenException(IToken expectedToken, IToken foundToken)
        : base($"Expecting '{expectedToken}' in expression, instead got: '{foundToken}'")
    {
        ExpectedToken = expectedToken;
        FoundToken = foundToken;
    }

    public IToken ExpectedToken { get; }
    public IToken FoundToken { get; }
}
