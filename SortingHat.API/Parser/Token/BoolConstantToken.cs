using Funcky.Lexer.Token;

namespace SortingHat.API.Parser.Token;

internal class BoolConstantToken(bool value) : IToken
{
    public bool Value { get; } = value;

    public override string ToString() => $"Bool Value: {Value}";
}

internal class TrueConstantToken : BoolConstantToken
{
    public TrueConstantToken()
        : base(true)
    {
    }
}

internal class FalseConstantToken : BoolConstantToken
{
    public FalseConstantToken()
        : base(false)
    {
    }
}
