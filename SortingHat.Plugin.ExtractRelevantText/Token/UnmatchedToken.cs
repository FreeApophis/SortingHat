using apophis.Lexer.Tokens;

namespace SortingHat.Plugin.ExtractRelevantText.Token
{
    class UnmatchedToken : IToken
    {
        private string _unmatchedText;

        public UnmatchedToken(string unmatchedText)
        {
            _unmatchedText = unmatchedText;
        }

        public override string ToString()
        {
            return $"Unmatched: {_unmatchedText}";
        }
    }
}
