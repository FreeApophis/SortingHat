﻿using System.Collections.Generic;
using System.Linq;

namespace SortingHat.API.Parser.Token
{
    public class TokenWalker
    {
        private readonly List<IToken> _tokens = new List<IToken>();
        private int _currentIndex;

        public bool ThereAreMoreTokens => _currentIndex < _tokens.Count;

        public TokenWalker(IEnumerable<IToken> tokens)
        {
            _tokens = tokens.ToList();
        }

        public IToken Pop()
        {
            return _tokens[_currentIndex++];
        }

        public IToken Peek()
        {
            return _tokens[_currentIndex];
        }
    }
}
