using System;
using Funcky.Extensions;
using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.OperatorType;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>The query parser is a pure function: a query gives its syntax tree, or the error with its position and what was expected there.</summary>
public sealed class ParserTest
{
    private readonly Parser _parser = Parser.Create();

    [Fact]
    public void GivenATagThenItIsATagNode()
        => Assert.Equal(":tax:2018", Assert.IsType<TagNode>(Valid(":tax:2018")).Tag);

    [Theory]
    [InlineData(":test or true and (:movie or not :blue)", "(:test ∨ (true ∧ (:movie ∨ ¬:blue)))")]
    [InlineData(":A || :B && :C", "(:A ∨ (:B ∧ :C))")]
    [InlineData("(:A or :B) and :C", "((:A ∨ :B) ∧ :C)")]
    public void GivenAQueryThenAndBindsStrongerThanOr(string query, string tree)
        => Assert.Equal(tree, Written(Valid(query), new LogicalOperatorType()));

    [Theory]
    [InlineData(":A || :B || :C || :D", "(((:A || :B) || :C) || :D)")]
    [InlineData(":A && :B && :C && :D", "(((:A && :B) && :C) && :D)")]
    public void GivenTheSameOperatorSeveralTimesThenItGroupsFromTheLeft(string query, string tree)
        => Assert.Equal(tree, Written(Valid(query), new ProgrammingOperatorType()));

    [Fact]
    public void GivenSeveralNotsThenEachOneNegates()
        => Assert.Equal("(not(not(not(:A))) and not(not(:B)))", Written(Valid("!!!:A && !!:B"), new TextOperatorType()));

    [Fact]
    public void GivenAQueryThenItsTreeEqualsTheSameTreeBuiltByHand()
        => Assert.Equal(
            new OrOperatorNode(new TagNode(":a"), new AndOperatorNode(new TagNode(":b"), new NotOperatorNode(new BooleanNode(true)))),
            Valid(":a or :b and not true"));

    [Fact]
    public void GivenAParsedQueryThenItIsWrittenAsTextQuery()
        => Assert.Equal("(:a or (:b and not(true)))", Valid(":a || :b && !true").ToString());

    [Fact]
    public void GivenTheSameQueryTwiceThenTheParserGivesTheSameResultBecauseItKeepsNoState()
    {
        _ = _parser.Parse(":a :b");

        Assert.Equal(":a", Assert.IsType<TagNode>(Valid(":a")).Tag);
    }

    [Theory]
    [InlineData(":a :b", "':b'", 3)]
    [InlineData(":a )", "')'", 3)]
    [InlineData(":a and :b :c", "':c'", 10)]
    [InlineData("(:a or :b) :c", "':c'", 11)]
    public void GivenSomethingAfterACompleteQueryThenTheQueryIsIllegalAtThatPosition(string query, string rest, int position)
    {
        var error = Error(query);

        Assert.Contains($"'{query}'", error.Message);
        Assert.Contains(rest, error.Message);
        Assert.Equal((position, false), (error.Position, error.IsIncomplete));
    }

    [Theory]
    [InlineData(":a and", "end")]
    [InlineData("(:a or :b", "'('")]
    [InlineData("", "end")]
    public void GivenAnIncompleteQueryThenTheErrorSaysWhatIsMissing(string query, string missing)
        => Assert.Contains(missing, Error(query).Message);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("(:tag and ")]
    [InlineData("not ")]
    [InlineData("(not ")]
    public void GivenAQueryThatEndsWhereAnOperandBelongsThenATagNotOrParenthesisIsExpected(string query)
    {
        Assert.True(Error(query).IsIncomplete);
        Assert.Equal([QueryPart.Tag, QueryPart.Not, QueryPart.OpenParenthesis], Error(query).Expected);
    }

    [Theory]
    [InlineData("(:tag")]
    [InlineData("(:test or ((:tag or :bla and :fun) and :x")]
    public void GivenAQueryThatEndsInsideParenthesesThenAnOperatorOrTheClosingParenthesisIsExpected(string query)
    {
        Assert.True(Error(query).IsIncomplete);
        Assert.Equal([QueryPart.And, QueryPart.Or, QueryPart.ClosedParenthesis], Error(query).Expected);
    }

    [Fact]
    public void GivenAnErrorInTheMiddleThenTheQueryIsNotIncomplete()
        => Assert.Equal((0, false), (Error(")").Position, Error(")").IsIncomplete));

    [Fact]
    public void GivenACharacterNoQueryCanContainThenTheQueryIsIllegalAtThatCharacter()
    {
        var error = Error(":a and #");

        Assert.Contains("'#' cannot be part of a query", error.Message);
        Assert.Equal(7, error.Position);
    }

    [Theory]
    [InlineData(":a and :b")]
    [InlineData("(:a or :b) and not :c")]
    [InlineData("true")]
    public void GivenACompleteQueryThenItParses(string query)
        => Assert.True(_parser.Parse(query).Match(left: False, right: True));

    private static string Written(ParseNode node, IOperatorType operatorType)
        => node.ToString(operatorType);

    private ParseNode Valid(string query)
        => Assert.Right(_parser.Parse(query));

    private ParseError Error(string query)
        => _parser.Parse(query).Match(left: Identity, right: node => throw new InvalidOperationException($"Expected an error, but '{query}' parsed to {node}."));
}
