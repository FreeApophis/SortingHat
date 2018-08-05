using SortingHat.API.Parser.Nodes;
using System;

namespace SortingHat.API.Parser
{
    /// <summary>
    /// This is a Recursive Descent Parser with the following Grammer in EBNF.
    /// 
    /// Expression   := Not Term { Or Term }
    /// Term         := Factor { And Factor }
    /// Factor       := Tag | BoolConstant | "(" Expression ")
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
        private TokenWalker _walker;
        private IParseNode _parseTree;

        public QueryParser(string expression)
        {
            _expression = expression;
        }


        public IParseNode Parse()
        {
            var tokens = new Tokenizer().Scan(_expression);
            _walker = new TokenWalker(tokens);

            _parseTree = ParseExpression();
            return _parseTree;
        }

        // Expression := Not Term { Or Term }
        public IParseNode ParseExpression()
        {
            IParseNode result;
            if (NextIs<NotToken>())
            {
                _walker.Pop();
                result = new NotOperatorNode(ParseTerm());
            }
            else
            {
                result = ParseTerm();
            }
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

        /// Factor     := Tag | "(" Expression ")
        private IParseNode ParseFactor()
        {
            if (NextIs<TagToken>())
            {
                var tagToken = _walker.Pop() as TagToken;
                return new TagNode(tagToken.Value);
            }

            if (NextIs<BoolConstantToken>()) {
                var boolToken = _walker.Pop() as BoolConstantToken;
                return new BooleanNode(boolToken.Value);
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
                throw new Exception("Expecting Real number or '(' in expression, instead got : " + (PeekNext() != null ? PeekNext().ToString() : "End of expression"));
            }
            _walker.Pop();
        }

        private Token PeekNext()
        {
            return _walker.ThereAreMoreTokens ? _walker.Peek() : null;
        }

        private void Consume(Type type)
        {
            var token = _walker.Pop();
            if (token.GetType() != type)
            {
                throw new Exception($"Expecting {type} but got {token.ToString()} ");
            }
        }

        private bool NextIs<Type>()
        {
            return _walker.ThereAreMoreTokens && _walker.Peek() is Type;
        }


    }
}
