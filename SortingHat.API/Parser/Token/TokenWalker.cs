using System.Collections.Generic;
using System.Linq;

namespace SortingHat.API.Parser.Token
{
    public class TokenWalker
    {
        private readonly List<Token.IToken> _tokens = new List<Token.IToken>();
        private int _currentIndex;

        public bool ThereAreMoreTokens => _currentIndex < _tokens.Count;

        public TokenWalker(IEnumerable<Token.IToken> tokens)
        {
            _tokens = tokens.ToList();
        }

        public Token.IToken Pop()
        {
            return _tokens[_currentIndex++];
        }

        public Token.IToken Peek()
        {
            return _tokens[_currentIndex];
        }
    }
}
