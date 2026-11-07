using System;
using System.Collections.Generic;
using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using Funcky.Lexer.Token;
using Funcky.Monads;
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
public class Parser : IParser
{
    private readonly List<IToken> _nextToken = [];
    private readonly LexerRuleBook _lexerRuleBook;
    private readonly ExpressionParser _expressionParser;

    /// <summary>The recursive descent parsers are internal, <see cref="ParserModule"/> or <see cref="Create"/> build the parser.</summary>
    internal Parser(LexerRuleBook lexerRuleBook, ExpressionParser expressionParser)
    {
        _lexerRuleBook = lexerRuleBook;
        _expressionParser = expressionParser;
    }

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
        return _expressionParser.Parse(walker);
    }

    public IEnumerable<IToken> NextToken()
    {
        return _nextToken;
    }

    /// <summary>The <paramref name="query"/> as a syntax tree, or why it is no valid query.</summary>
    public Either<string, IParseNode> ParseQuery(string query)
    {
        try
        {
            return Either<string, IParseNode>.Right(ParseAll(query));
        }
        catch (ExpectedTokenException exception)
        {
            return Either<string, IParseNode>.Left($"'{query}' is not a valid query: {Problem(exception)}.");
        }
        catch (Exception exception)
        {
            return Either<string, IParseNode>.Left($"'{query}' is not a valid query: {exception.Message}");
        }
    }

    public IParseNode? Parse(string expression)
    {
        try
        {
            _nextToken.Clear();
            var ast = ParseAll(expression);
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

    private static string Problem(ExpectedTokenException exception)
        => (exception.ExpectedToken, exception.FoundToken) switch
        {
            (EpsilonToken, var found) => $"{Describe(found)} follows a complete query, combine the parts with 'and' or 'or'",
            (ClosedParenthesisToken, EpsilonToken) => "a '(' is not closed",
            (ClosedParenthesisToken, var found) => $"{Describe(found)} is where an operator or ')' belongs",
            (_, EpsilonToken) => "the query ends where a tag, 'not' or '(' is expected",
            (_, var found) => $"{Describe(found)} is where a tag, 'not' or '(' is expected",
        };

    private static string Describe(IToken token)
        => token switch
        {
            TagToken tag => $"'{tag.Value}'",
            AndToken => "'and'",
            OrToken => "'or'",
            NotToken => "'not'",
            OpenParenthesisToken => "'('",
            ClosedParenthesisToken => "')'",
            BoolConstantToken constant => constant.Value ? "'true'" : "'false'",
            _ => $"'{token}'",
        };

    /// <summary>The whole query is one expression: anything after it makes the query illegal instead of being ignored.</summary>
    private IParseNode ParseAll(string query)
    {
        var walker = _lexerRuleBook.Scan(query).Walker;
        var ast = Parse(walker);

        return walker.NextIs<EpsilonToken>()
            ? ast
            : throw new ExpectedTokenException(new EpsilonToken(), walker.Peek().Token);
    }
}
