﻿using SortingHat.API.Parser.Nodes;
using System;
using System.Collections.Generic;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    /// <summary>
    /// This is a Recursive Descent Parser with the following Grammer in EBNF.
    /// 
    /// Expression   := Term { Or Term }
    /// Term         := Factor { And Factor }
    /// Factor       := Tag | BoolConstant | Not Factor | "(" Expression ") | Predicate
    /// Predicate    := TODO: not yet defined...
    /// Tag          := ":"  Any non whitespace { Any non whitespace }
    /// Or           := "or" | "||" | "∨"
    /// And          := "and" | "&&"| "∧"
    /// Not          := "not" | "!" | "¬"
    /// BoolConstant := True | False
    /// True         := "true" | "1"
    /// False        := "false" | "1"
    /// </summary>
    public class QueryParser
    {
        private readonly string _expression;
        private readonly List<IToken> _nextToken = new List<IToken>();

        private TokenWalker _walker;
        private IParseNode _parseTree;

        public QueryParser(string expression)
        {
            _expression = expression;
        }

        public IParseNode Parse()
        {
            if (string.IsNullOrWhiteSpace(_expression))
            {
                _nextToken.Add(new TagToken(null));
                _nextToken.Add(new NotToken());
                return null;
            }

            var tokens = new Tokenizer().Scan(_expression);
            _walker = new TokenWalker(tokens);

            _parseTree = ParseExpression();
            return _parseTree;
        }

        public IEnumerable<IToken> NextToken()
        {
            return _nextToken;
        }

        // Expression := Term { Or Term }
        private IParseNode ParseExpression()
        {
            var result = ParseTerm();
            while (NextIs<OrToken>())
            {
                var op = _walker.Pop();
                if (op is OrToken)
                {
                    result = new OrOperatorNode(result, ParseTerm());
                }
            }
            return result;
        }

        // Term       := Factor { And Factor }
        private IParseNode ParseTerm()
        {
            var result = ParseFactor();
            while (NextIs<AndToken>())
            {
                var op = _walker.Pop();
                if (op is AndToken)
                {
                    result = new AndOperatorNode(result, ParseFactor());
                }
            }

            return result;
        }

        // Factor       := Tag | BoolConstant | Not Factor | "(" Expression ") 
        private IParseNode ParseFactor()
        {
            if (NextIs<TagToken>())
            {
                var tagToken = _walker.Pop() as TagToken;
                return new TagNode(tagToken.Value);
            }

            if (NextIs<BoolConstantToken>())
            {
                var boolToken = _walker.Pop() as BoolConstantToken;
                return new BooleanNode(boolToken.Value);
            }

            if (NextIs<NotToken>())
            {
                _walker.Pop();
                return new NotOperatorNode(ParseFactor());
            }

            ExpectOpeningParenthesis();
            var result = ParseExpression();
            ExpectClosingParenthesis();

            return result;
        }

        private void ExpectClosingParenthesis()
        {
            if (!(NextIs<ClosedParenthesisToken>()))
            {
                throw new Exception("Expecting ')' in expression, instead got: " + (PeekNext() != null ? PeekNext().ToString() : "End of expression"));
            }
            _walker.Pop();
        }

        private void ExpectOpeningParenthesis()
        {
            if (!NextIs<OpenParenthesisToken>())
            {
                throw new Exception("Expecting '(' in expression, instead got : " + (PeekNext() != null ? PeekNext().ToString() : "End of expression"));
            }
            _walker.Pop();
        }

        private IToken PeekNext()
        {
            return _walker.ThereAreMoreTokens ? _walker.Peek() : null;
        }

        private bool NextIs<TType>()
        {
            return _walker.ThereAreMoreTokens && _walker.Peek() is TType;
        }


    }
}
