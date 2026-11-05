using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public static class LexemeWalkerExtensions
    {
        extension(ILexemeWalker walker)
        {
            public void ExpectClosingParenthesis()
            {
                if (!walker.NextIs<ClosedParenthesisToken>())
                {
                    throw new ExpectedTokenException(new ClosedParenthesisToken(), walker.Peek().Token);
                }
                walker.Pop();
            }

            public void ExpectOpeningParenthesis()
            {
                if (!walker.NextIs<OpenParenthesisToken>())
                {
                    throw new ExpectedTokenException(new OpenParenthesisToken(), walker.Peek().Token);
                }
                walker.Pop();
            }
        }
    }
}
