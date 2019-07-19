using System.Collections.Generic;
using System.Text;
using apophis.Lexer;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public class LexerRules : ILexerRules
    {
        public IEnumerable<ILexerRule> GetRules()
        {
            yield return new LexerRule(char.IsWhiteSpace, ConsumeWhiteSpace);
            yield return new LexerRule(c => c == ':', ConsumeTag);
            yield return new SimpleLexerRule<OrToken>("∨");
            yield return new SimpleLexerRule<OrToken>("||");
            yield return new SimpleLexerRule<OrToken>("or");
            yield return new SimpleLexerRule<AndToken>("∧");
            yield return new SimpleLexerRule<AndToken>("&&");
            yield return new SimpleLexerRule<AndToken>("and");
            yield return new SimpleLexerRule<NotToken>("¬");
            yield return new SimpleLexerRule<NotToken>("!");
            yield return new SimpleLexerRule<NotToken>("not");
            yield return new SimpleLexerRule<TrueConstantToken>("true");
            yield return new SimpleLexerRule<TrueConstantToken>("1");
            yield return new SimpleLexerRule<FalseConstantToken>("false");
            yield return new SimpleLexerRule<FalseConstantToken>("0");
            yield return new SimpleLexerRule<OpenParenthesisToken>("(");
            yield return new SimpleLexerRule<ClosedParenthesisToken>(")");
        }

        private bool IsLegalCharacter(char currentCharacter)
        {
            return char.IsLetter(currentCharacter)
                   || char.IsNumber(currentCharacter)
                   || currentCharacter == '-'
                   || currentCharacter == '_'
                   || currentCharacter == ':';
        }

        private Lexem ConsumeTag(ILexerReader reader)
        {
            var startPosition = reader.Position;

            StringBuilder stringBuilder = new StringBuilder();

            while (reader.Peek().Match(false, IsLegalCharacter))
            {
                reader.Read().Match(none: stringBuilder, some: c => stringBuilder.Append(c));
            }

            return new Lexem(new TagToken(stringBuilder.ToString()), new Position(startPosition, reader.Position - startPosition));
        }

        private static Lexem ConsumeWhiteSpace(ILexerReader reader)
        {
            var startPosition = reader.Position;

            while (reader.Peek().Match(false, char.IsWhiteSpace))
            {
                // we are not interested in what kind of whitespace, so we just discard the result
                reader.Read();
            }

            return new Lexem(new WhiteSpaceToken(), new Position(startPosition, reader.Position - startPosition));
        }
    }
}