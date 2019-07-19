using System;
using apophis.Lexer;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public static class TokenWalkerExtensions
    {
        public static void ExpectClosingParenthesis(this TokenWalker walker)
        {
            if (!(walker.NextIs<ClosedParenthesisToken>()))
            {
                throw new Exception("Expecting ')' in expression, instead got: " + (walker.Peek() != null ? walker.Peek().ToString() : "End of expression"));
            }
            walker.Pop();
        }

        public static void ExpectOpeningParenthesis(this TokenWalker walker)
        {
            if (!walker.NextIs<OpenParenthesisToken>())
            {
                throw new Exception("Expecting Real number or '(' in expression, instead got : " + (walker.Peek() != null ? walker.Peek().ToString() : "End of expression"));
            }
            walker.Pop();
        }
    }
}