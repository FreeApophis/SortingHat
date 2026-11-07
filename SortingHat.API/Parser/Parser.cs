using System.Collections.Generic;
using Funcky.Extensions;
using Funcky.Lexer;
using Funcky.Lexer.Exceptions;
using Funcky.Monads;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser;

/// <summary>
/// Parses a search query into its syntax tree. It keeps no state: the same query always gives the same result. The grammar in EBNF:
///
/// Expression   := Term { Or Term }
/// Term         := Factor { And Factor }
/// Factor       := Tag | BoolConstant | Not Factor | "(" Expression ")"
/// Tag          := ":" Level { ":" Level }, a level of letters, digits, '-', '_', '+' and '.'
/// Or           := "or" | "||" | "∨"
/// And          := "and" | "&amp;&amp;" | "∧"
/// Not          := "not" | "!" | "¬"
/// BoolConstant := True | False
/// True         := "true" | "1"
/// False        := "false" | "0".
/// </summary>
public sealed class Parser
{
    private readonly LexerRuleBook _lexerRuleBook;

    private Parser(LexerRuleBook lexerRuleBook)
        => _lexerRuleBook = lexerRuleBook;

    public static Parser Create()
        => new(LexerRules.Create());

    /// <summary>The <paramref name="query"/> as a syntax tree, or why it is no valid query.</summary>
    public Either<ParseError, IParseNode> Parse(string query)
        => Lexemes(query).SelectMany(lexemes => QueryGrammar.Parse(query, lexemes));

    /// <summary>The lexer reports a character no rule knows with an exception, the only one the parser expects.</summary>
    private Either<ParseError, IReadOnlyList<Lexeme>> Lexemes(string query)
    {
        try
        {
            return Either<ParseError, IReadOnlyList<Lexeme>>.Right([.. _lexerRuleBook.Scan(query).Lexemes]);
        }
        catch (UnknownTokenException exception)
        {
            var position = exception.Position.StartPosition;

            return Either<ParseError, IReadOnlyList<Lexeme>>.Left(new ParseError(query, position, $"{query.ElementAtOrNone(position).Match(none: "a character", some: character => $"'{character}'")} cannot be part of a query", [], false));
        }
    }
}
