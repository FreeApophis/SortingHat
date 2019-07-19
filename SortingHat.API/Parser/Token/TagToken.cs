using apophis.Lexer.Tokens;

namespace SortingHat.API.Parser.Token
{
    public class TagToken : IToken
    {
        public TagToken(string? value)
        {
            Value = value;
        }

        public string? Value { get; }

        public override string ToString() => $"Tag: {Value}";
    }
}