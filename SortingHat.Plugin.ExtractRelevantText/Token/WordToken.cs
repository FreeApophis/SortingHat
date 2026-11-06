using Funcky.Lexer.Token;

namespace SortingHat.Plugin.ExtractRelevantText.Token;

internal class WordToken : IToken
{
    public WordToken(string word)
    {
        Word = word;
    }

    public string Word { get; }

    public override string ToString()
    {
        return $"Word: {Word}";
    }
}
