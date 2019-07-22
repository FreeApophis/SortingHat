using System.Collections.Generic;
using System.Text;
using apophis.Lexer;
using apophis.Lexer.Rules;
using SortingHat.Plugin.ExtractRelevantText.Token;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public class LexerRules : ILexerRules
    {
        public IEnumerable<ILexerRule> GetRules()
        {
            yield return new LexerRule(char.IsWhiteSpace, ConsumeWhiteSpace);
            yield return new LexerRule(char.IsLetter, ConsumeWord);
        }

        private Lexem ConsumeWord(ILexerReader reader)
        {
            var startPosition = reader.Position;
            var word = new StringBuilder();

            while (reader.Peek().Match(false, char.IsLetter))
            {
                // we are not interested in what kind of whitespace, so we just discard the result
                word.Append(reader.Read());
            }

            return new Lexem(new WordToken(word.ToString()), new Position(startPosition, reader.Position - startPosition));
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
