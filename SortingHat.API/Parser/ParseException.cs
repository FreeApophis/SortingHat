using System;
using Funcky.Lexer.Token;

namespace SortingHat.API.Parser;

public class ParseException(string message) : Exception(message)
{
}

internal class ExpectedTokenException(IToken expectedToken, IToken foundToken) : Exception($"Expecting '{expectedToken}' in expression, instead got: '{foundToken}'")
{
    public IToken ExpectedToken { get; } = expectedToken;
    public IToken FoundToken { get; } = foundToken;
}
