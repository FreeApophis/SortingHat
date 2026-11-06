using Funcky.Lexer.Token;

namespace SortingHat.Plugin.ExtractRelevantText.Token;

internal class UnmatchedToken(string unmatchedText) : IToken
{
    private string _unmatchedText = unmatchedText;

    public override string ToString()
    {
        return $"Unmatched: {_unmatchedText}";
    }
}
