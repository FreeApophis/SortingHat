using System;
using System.Collections.Generic;
using System.Linq;
using apophis.Lexer;
using apophis.Lexer.Tokens;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;
using Tokenizer = apophis.Lexer.Tokenizer;
using TokenWalker = apophis.Lexer.TokenWalker;

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
    /// And          := "and" | "&amp;&amp;"| "∧"
    /// Not          := "not" | "!" | "¬"
    /// BoolConstant := True | False
    /// True         := "true" | "1"
    /// False        := "false" | "0"
    /// </summary>
    public class Parser : IParser
    {
        private readonly TokenWalker _tokenWalker;
        private readonly ExpressionParser _expressionParser;
        private readonly List<IToken> _nextToken = new List<IToken>();

        public Parser(TokenWalker tokenWalker, ExpressionParser expressionParser)
        {
            _tokenWalker = tokenWalker;
            _expressionParser = expressionParser;
        }

        public static Parser Create()
        {
            // Create the object tree without DI Framework
            var expressionParser = new ExpressionParser();
            var factorParser = new FactorParser(expressionParser);
            var termParser = new TermParser(factorParser);
            expressionParser.TermParser = termParser;
            var lexerRules = new LexerRules();
            ILinePositionCalculator NewLinePositionCalculator(List<Lexem> lexems) => new LinePositionCalculator(lexems);
            ILexerReader NewLexerReader(string expression) => new LexerReader(expression);
            var tokenizer = new Tokenizer(lexerRules, NewLexerReader, NewLinePositionCalculator);
            IToken NewEpsilonToken() => new EpsilonToken();
            var tokenWalker = new TokenWalker(tokenizer, NewEpsilonToken, NewLinePositionCalculator);

            return new Parser(tokenWalker, expressionParser);
        }

        public IParseNode Parse(TokenWalker walker)
        {
            return _expressionParser.Parse(walker);
        }

        public IEnumerable<IToken> NextToken()
        {
            return _nextToken;
        }

        public IParseNode? Parse(string expression)
        {
            try
            {
                _nextToken.Clear();
                _tokenWalker.Scan(expression, lexems => lexems.Where(t => t.Token.GetType() != typeof(WhiteSpaceToken)));
                var ast = Parse(_tokenWalker);
                _nextToken.Add(new AndToken());
                _nextToken.Add(new OrToken());
                return ast;

            }
            catch (ExpectedTokenException e)
            {
                IllegalExpression = true;
                if (e.ExpectedToken is OpenParenthesisToken && e.FoundToken is EpsilonToken)
                {
                    _nextToken.Add(new TagToken(null));
                    _nextToken.Add(new NotToken());
                    _nextToken.Add(new OpenParenthesisToken());
                }

                if (e.ExpectedToken is ClosedParenthesisToken && e.FoundToken is EpsilonToken)
                {
                    _nextToken.Add(new AndToken());
                    _nextToken.Add(new OrToken());
                    _nextToken.Add(new ClosedParenthesisToken());
                }

                return null;
            }
            catch (Exception)
            {
                IllegalExpression = true;
                return null;
            }

        }

        public bool IllegalExpression { get; private set; }
    }
}
