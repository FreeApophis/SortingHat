using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using System.Linq;
using SortingHat.API.Parser.OperatorType;
using SortingHat.API.Parser.Token;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        private Parser _parser = Parser.Create();

        [Fact]
        public void EmptySearch()
        {
            var root = _parser.Parse(" ");


            Assert.True(_parser.IllegalExpression);
            Assert.Null(root);
        }

        [Fact]
        public void SingleTag()
        {
            var root = _parser.Parse(":tax:2018");

            Assert.IsType<TagNode>(root);
            Assert.False(_parser.IllegalExpression);
            Assert.Equal(":tax:2018", (root as TagNode)?.Tag);
        }

        [Fact]
        public void SimpleQuery()
        {
            var root = _parser.Parse(":test or true and (:movie or not :blue)");
            var visitor = new ToStringVisitor(new LogicalOperatorType());

            root.Accept(visitor);
            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);
            Assert.False(_parser.IllegalExpression);
        }

        [Fact]
        public void PrecedenceTest()
        {
            var root = _parser.Parse(":A || :B && :C");
            var visitor = new ToStringVisitor(new LogicalOperatorType());

            root.Accept(visitor);
            Assert.Equal("(:A ∨ (:B ∧ :C))", visitor.Result);
        }

        [Fact]
        public void OrAssociativityTest()
        {
            var root = _parser.Parse(":A || :B || :C || :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            root.Accept(visitor);
            Assert.Equal("(((:A || :B) || :C) || :D)", visitor.Result);
        }

        [Fact]
        public void AndAssociativityTest()
        {
            var root = _parser.Parse(":A && :B && :C && :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            root.Accept(visitor);
            Assert.Equal("(((:A && :B) && :C) && :D)", visitor.Result);
        }

        [Fact]
        public void MultipleNotTest()
        {
            var root = _parser.Parse("!!!:A && !!:B");
            var visitor = new ToStringVisitor(new TextOperatorType());

            root.Accept(visitor);
            Assert.Equal("(not(not(not(:A))) and not(not(:B)))", visitor.Result);
        }

        [Fact]
        public void EmptyStringNextNode()
        {
            var root = _parser.Parse("");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagNextNode()
        {
            var root = _parser.Parse(":tag");
            var next = _parser.NextToken();

            Assert.Equal(2, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void TagOpenParanthesisNextNode()
        {
            var root = _parser.Parse("(:tag");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void TagAndNextNode()
        {
            var root = _parser.Parse("(:tag and ");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void NotNextNode()
        {
            var root = _parser.Parse("not ");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void OpenParentesisNotNextNode()
        {
            var root = _parser.Parse("(not ");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagComplexNextNode()
        {
            var root = _parser.Parse("(:test or ((:tag or :bla and :fun) and :x");
            var next = _parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void CompleteExpressionNextNode()
        {
            var root = _parser.Parse("(:tag and :fun)");
            var next = _parser.NextToken();

            Assert.Equal(2, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void IllegalNextNode()
        {
            var root = _parser.Parse(")");
            var next = _parser.NextToken();

            Assert.Empty(next);
        }

        private const string SelectStatement = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path
FROM Files
JOIN FilePaths ON FilePaths.FileID = Files.ID
WHERE (0 OR (0 AND 0))
GROUP BY FilePaths.ID";

        [Fact]
        public void SQLQuery()
        {
            var root = _parser.Parse(":tax:2016 || :cool && :audio:original");
            var visitor = new DB.SearchQueryVisitor(new MockDatabase(), new MockTagParser());

            root.Accept(visitor);
            Assert.Equal(SelectStatement, visitor.Result);
        }
    }
}
