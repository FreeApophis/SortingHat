using System.Linq;
using Funcky.Lexer;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public static class LexerRules
    {
        public static LexerRuleBook Create()
            => LexerRuleBook.Builder
                .AddRule(char.IsWhiteSpace, ConsumeWhiteSpace)
                .AddRule(c => c == ':', ConsumeTag)
                .AddSimpleRule<OrToken>("∨")
                .AddSimpleRule<OrToken>("||")
                .AddSimpleRule<OrToken>("or")
                .AddSimpleRule<AndToken>("∧")
                .AddSimpleRule<AndToken>("&&")
                .AddSimpleRule<AndToken>("and")
                .AddSimpleRule<NotToken>("¬")
                .AddSimpleRule<NotToken>("!")
                .AddSimpleRule<NotToken>("not")
                .AddSimpleRule<TrueConstantToken>("true")
                .AddSimpleRule<TrueConstantToken>("1")
                .AddSimpleRule<FalseConstantToken>("false")
                .AddSimpleRule<FalseConstantToken>("0")
                .AddSimpleRule<OpenParenthesisToken>("(")
                .AddSimpleRule<ClosedParenthesisToken>(")")
                // the parser is not interested in whitespace
                .WithPostProcess(lexemes => lexemes.Where(lexeme => !(lexeme.Token is WhiteSpaceToken)))
                .WithEpsilonToken<EpsilonToken>()
                .Build();

        private static bool IsLegalCharacter(char currentCharacter)
        {
            return char.IsLetter(currentCharacter)
                   || char.IsNumber(currentCharacter)
                   || currentCharacter == '-'
                   || currentCharacter == '_'
                   || currentCharacter == ':';
        }

        private static Lexeme ConsumeTag(ILexemeBuilder builder)
        {
            while (builder.Peek().Match(false, IsLegalCharacter))
            {
                builder.Retain();
            }

            return builder.Build(new TagToken(builder.CurrentToken));
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
