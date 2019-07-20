using System;
using apophis.Lexer;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public static class TokenWalkerExtensions
    {
        public static void ExpectClosingParenthesis(this TokenWalker walker)
        {
            if (!walker.NextIs<ClosedParenthesisToken>())
            {
                throw new ExpectedTokenException(new ClosedParenthesisToken(), walker.Peek().Token ?? new EpsilonToken());
            }
            walker.Pop();
        }

        public static void ExpectOpeningParenthesis(this TokenWalker walker)
        {
            if (!walker.NextIs<OpenParenthesisToken>())
            {
                throw new ExpectedTokenException(new OpenParenthesisToken(), walker.Peek().Token ?? new EpsilonToken());
            }
            walker.Pop();
        }
    }
}