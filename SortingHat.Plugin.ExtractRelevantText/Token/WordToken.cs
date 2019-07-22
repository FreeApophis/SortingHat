using apophis.Lexer.Tokens;

namespace SortingHat.Plugin.ExtractRelevantText.Token
{
    internal class WordToken : IToken
    {
        public WordToken(string word)
        {
            Word = word;
        }
        public string Word { get; }
    }
}
