using Funcky.Lexer.Token;

namespace SortingHat.API.Parser.Token;

public class BoolConstantToken(bool value) : IToken
{
    public bool Value { get; } = value;

    public override string ToString() => $"Bool Value: {Value}";
}

public class TrueConstantToken : BoolConstantToken
{
    public TrueConstantToken()
        : base(true)
    {
    }
}

public class FalseConstantToken : BoolConstantToken
{
    public FalseConstantToken()
        : base(false)
    {
    }
}
