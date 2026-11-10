using System.Collections.Generic;
using Funcky.Extensions;
using Funcky.Lexer;
using Funcky.Lexer.Token;
using Funcky.Monads;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser;

/// <summary>
/// The recursive descent parser for the grammar described at <see cref="Parser"/>, as pure functions over the lexemes of a query:
/// every function takes the position of the next lexeme and returns the parsed node with the position after it, or the error.
/// </summary>
internal static class QueryGrammar
{
    private static readonly IReadOnlyList<QueryPart> Operand = [QueryPart.Tag, QueryPart.Not, QueryPart.OpenParenthesis];

    private static readonly IReadOnlyList<QueryPart> AfterOperandInParentheses = [QueryPart.And, QueryPart.Or, QueryPart.ClosedParenthesis];

    private static readonly IReadOnlyList<QueryPart> AfterOperand = [QueryPart.And, QueryPart.Or];

    /// <summary>The whole query is one expression: anything after it makes the query illegal instead of being ignored.</summary>
    public static Either<ParseError, ParseNode> Parse(string query, IReadOnlyList<Lexeme> lexemes)
    {
        var input = new Input(query, lexemes);

        return Expression(input, 0).SelectMany(parsed => input.IsEnd(parsed.Next)
            ? Either<ParseError, ParseNode>.Right(parsed.Node)
            : Either<ParseError, ParseNode>.Left(input.Error(parsed.Next, $"{input.Describe(parsed.Next)} follows a complete query, combine the parts with 'and' or 'or'", AfterOperand)));
    }

    /// <summary>Expression := Term { Or Term }.</summary>
    private static Either<ParseError, Parsed> Expression(Input input, int at)
        => Term(input, at).SelectMany(term => MoreTerms(input, term));

    private static Either<ParseError, Parsed> MoreTerms(Input input, Parsed left)
        => input.TokenAt(left.Next) is OrToken
            ? Term(input, left.Next + 1).SelectMany(right => MoreTerms(input, new Parsed(new OrOperatorNode(left.Node, right.Node), right.Next)))
            : left;

    /// <summary>Term := Factor { And Factor }.</summary>
    private static Either<ParseError, Parsed> Term(Input input, int at)
        => Factor(input, at).SelectMany(factor => MoreFactors(input, factor));

    private static Either<ParseError, Parsed> MoreFactors(Input input, Parsed left)
        => input.TokenAt(left.Next) is AndToken
            ? Factor(input, left.Next + 1).SelectMany(right => MoreFactors(input, new Parsed(new AndOperatorNode(left.Node, right.Node), right.Next)))
            : left;

    /// <summary>Factor := Tag | BoolConstant | Not Factor | "(" Expression ")".</summary>
    private static Either<ParseError, Parsed> Factor(Input input, int at)
        => input.TokenAt(at) switch
        {
            TagToken { Value: { } tag } => new Parsed(new TagNode(tag), at + 1),
            BoolConstantToken constant => new Parsed(new BooleanNode(constant.Value), at + 1),
            NotToken => Factor(input, at + 1).Select(factor => factor with { Node = new NotOperatorNode(factor.Node) }),
            OpenParenthesisToken => Expression(input, at + 1).SelectMany(inner => ClosingParenthesis(input, inner)),
            _ => Either<ParseError, Parsed>.Left(input.Error(at, input.IsEnd(at) ? "the query ends where a tag, 'not' or '(' is expected" : $"{input.Describe(at)} is where a tag, 'not' or '(' is expected", Operand)),
        };

    private static Either<ParseError, Parsed> ClosingParenthesis(Input input, Parsed inner)
        => input.TokenAt(inner.Next) is ClosedParenthesisToken
            ? inner with { Next = inner.Next + 1 }
            : Either<ParseError, Parsed>.Left(input.Error(inner.Next, input.IsEnd(inner.Next) ? "a '(' is not closed" : $"{input.Describe(inner.Next)} is where an operator or ')' belongs", AfterOperandInParentheses));

    /// <param name="Node">The parsed part of the query.</param>
    /// <param name="Next">The position of the lexeme after it.</param>
    private sealed record Parsed(ParseNode Node, int Next);

    /// <summary>The lexemes of the query; reading past the last one gives the end.</summary>
    private sealed record Input(string Query, IReadOnlyList<Lexeme> Lexemes)
    {
        public IToken TokenAt(int index)
            => Lexemes.ElementAtOrNone(index).Match(none: () => new EpsilonToken(), some: lexeme => lexeme.Token);

        public bool IsEnd(int index)
            => TokenAt(index) is EpsilonToken;

        public ParseError Error(int index, string reason, IReadOnlyList<QueryPart> expected)
            => new(Query, Position(index), reason, expected, IsEnd(index));

        public string Describe(int index)
            => TokenAt(index) switch
            {
                TagToken tag => $"'{tag.Value}'",
                AndToken => "'and'",
                OrToken => "'or'",
                NotToken => "'not'",
                OpenParenthesisToken => "'('",
                ClosedParenthesisToken => "')'",
                BoolConstantToken constant => constant.Value ? "'true'" : "'false'",
                var token => $"'{token}'",
            };

        private int Position(int index)
            => IsEnd(index)
                ? Query.Length
                : Lexemes.ElementAtOrNone(index).Match(none: Query.Length, some: lexeme => lexeme.Position.StartPosition);
    }
}
