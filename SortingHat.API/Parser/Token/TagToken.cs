using Funcky.Lexer.Token;

namespace SortingHat.API.Parser.Token;

public class TagToken(string? value) : IToken
{
    public string? Value { get; } = value;

    public override string ToString() => $"Tag: {Value}";
}