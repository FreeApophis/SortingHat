﻿using apophis.Lexer.Tokens;

namespace SortingHat.API.Parser.Token
{
    public class EpsilonToken : IToken
    {
        public override string ToString() => "ε : End of expression";
    }
}
