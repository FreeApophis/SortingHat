using Funcky.Lexer.Token;

namespace SortingHat.Plugin.ExtractRelevantText.Token;

internal class WordToken(string word) : IToken
{
    public string Word { get; } = word;

    public override string ToString()
    {
        return $"Word: {Word}";
    }
}
