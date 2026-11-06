using System;
using System.Collections.Generic;
using Funcky.Lexer;
using Funcky.Lexer.Token;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

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
/// False        := "false" | "0".
/// </summary>
public class Parser(LexerRuleBook lexerRuleBook, ExpressionParser expressionParser) : IParser
{
    private readonly List<IToken> _nextToken = [];

    public bool IllegalExpression { get; private set; }

    public static Parser Create()
    {
        // Create the object tree without DI Framework
        var expressionParser = new ExpressionParser();
        var factorParser = new FactorParser(expressionParser);
        var termParser = new TermParser(factorParser);
        expressionParser.TermParser = termParser;

        return new Parser(LexerRules.Create(), expressionParser);
    }

    public IParseNode Parse(ILexemeWalker walker)
    {
        return expressionParser.Parse(walker);
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
            var ast = Parse(lexerRuleBook.Scan(expression).Walker);
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
}
