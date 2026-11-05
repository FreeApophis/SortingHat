using Funcky.Lexer;
using SortingHat.Plugin.ExtractRelevantText.Token;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public static class LexerRules
    {
        public static LexerRuleBook Create()
            => LexerRuleBook.Builder
                .AddRule(char.IsWhiteSpace, ConsumeWhiteSpace, 1)
                .AddRule(char.IsLetter, ConsumeWord, 1)
                .AddSimpleRule<PeriodToken>(".")
                .AddSimpleRule<QuestionMarkToken>("?")
                .AddSimpleRule<ExclamationMarkToken>("!")
                .AddSimpleRule<ColonToken>(":")
                .AddSimpleRule<SemiColonToken>(";")
                .AddSimpleRule<CommaToken>(",")
                .AddRule(NotWhiteSpace, ConsumeRest)
                .WithEpsilonToken<EpsilonToken>()
                .Build();

        private static Lexeme ConsumeRest(ILexemeBuilder builder)
        {
            while (builder.Peek().Match(false, NotWhiteSpace))
            {
                builder.Retain();
            }

            return builder.Build(new UnmatchedToken(builder.CurrentToken));
        }

        private static bool NotWhiteSpace(char character)
        {
            return char.IsWhiteSpace(character) == false;
        }

        private static Lexeme ConsumeWord(ILexemeBuilder builder)
        {
            while (builder.Peek().Match(false, char.IsLetter))
            {
                builder.Retain();
            }

            return builder.Build(new WordToken(builder.CurrentToken));
        }

        private static Lexeme ConsumeWhiteSpace(ILexemeBuilder builder)
        {
            while (builder.Peek().Match(false, char.IsWhiteSpace))
            {
                // we are not interested in what kind of whitespace, so we just discard the result
                builder.Discard();
            }

            return builder.Build(new WhiteSpaceToken());
        }
    }
}
