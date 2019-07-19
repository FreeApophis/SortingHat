using apophis.Lexer.Tokens;

namespace SortingHat.API.Parser.Token
{
    public class BoolConstantToken : IToken
    {
        public BoolConstantToken(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override string ToString() => $"Bool Value: {Value}";
    }

    public class TrueConstantToken : BoolConstantToken
    {
        public TrueConstantToken() : base(true)
        {
        }
    }

    public class FalseConstantToken : BoolConstantToken
    {
        public FalseConstantToken() : base(false)
        {
        }
    }

}
