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
            yield return new LexerRule(char.IsWhiteSpace, ConsumeWhiteSpace, 1);
            yield return new LexerRule(char.IsLetter, ConsumeWord, 1);
            yield return new SimpleLexerRule<PeriodToken>(".");
            yield return new SimpleLexerRule<QuestionMarkToken>("?");
            yield return new SimpleLexerRule<ExclamationMarkToken>("!");
            yield return new SimpleLexerRule<ColonToken>(":");
            yield return new SimpleLexerRule<SemiColonToken>(";");
            yield return new SimpleLexerRule<CommaToken>(",");
            yield return new LexerRule(NotWhiteSpace, ConsumeRest);
        }

        private Lexem ConsumeRest(ILexerReader reader)
        {
            var startPosition = reader.Position;
            var unmatchedText = new StringBuilder();

            while (reader.Peek().Match(false, NotWhiteSpace))
            {
                reader.Read().AndThen(c => unmatchedText.Append(c));
            }

            return new Lexem(new UnmatchedToken(unmatchedText.ToString()), new Position(startPosition, reader.Position - startPosition));
        }

        private bool NotWhiteSpace(char character)
        {
            return char.IsWhiteSpace(character) == false;
        }

        private Lexem ConsumeWord(ILexerReader reader)
        {
            var startPosition = reader.Position;
            var word = new StringBuilder();

            while (reader.Peek().Match(false, char.IsLetter))
            {
                reader.Read().AndThen(c => word.Append(c));
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
