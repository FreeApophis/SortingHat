using System;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }

    public class ExpectedTokenException : Exception
    {
        public IToken ExpectedToken { get; }
        public IToken FoundToken { get; }

        public ExpectedTokenException(IToken expectedToken, IToken foundToken) :
            base($"Expecting '{expectedToken}' in expression, instead got: '{foundToken}'")
        {
            ExpectedToken = expectedToken;
            FoundToken = foundToken;
        }
    }
}
