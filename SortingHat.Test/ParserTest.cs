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
        [Fact]
        public void EmptySearch()
        {
            var parser = new QueryParser(" ");
            var ir = parser.Parse();

            Assert.True(parser.IllegalExpression);
            Assert.Null(ir);
        }

        [Fact]
        public void SingleTag()
        {
            var parser = new QueryParser(":tax:2018");
            var parseTree = parser.Parse();

            Assert.IsType<TagNode>(parseTree);
            Assert.False(parser.IllegalExpression);
            Assert.Equal(":tax:2018", (parseTree as TagNode).Tag);
        }

        [Fact]
        public void SimpleQuery()
        {
            var parser = new QueryParser(":test or true and (:movie or not :blue)");
            var visitor = new ToStringVisitor(new LogicalOperatorType());

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);
            Assert.False(parser.IllegalExpression);
        }

        [Fact]
        public void PrecedenceTest()
        {
            var parser = new QueryParser(":A || :B && :C");
            var visitor = new ToStringVisitor(new LogicalOperatorType());

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:A ∨ (:B ∧ :C))", visitor.Result);
        }

        [Fact]
        public void OrAssociativityTest()
        {
            var parser = new QueryParser(":A || :B || :C || :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(((:A || :B) || :C) || :D)", visitor.Result);
        }

        [Fact]
        public void AndAssociativityTest()
        {
            var parser = new QueryParser(":A && :B && :C && :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(((:A && :B) && :C) && :D)", visitor.Result);
        }

        [Fact]
        public void MultipleNotTest()
        {
            var parser = new QueryParser("!!!:A && !!:B");
            var visitor = new ToStringVisitor(new TextOperatorType());

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(not(not(not(:A))) and not(not(:B)))", visitor.Result);
        }

        [Fact]
        public void EmptyStringNextNode()
        {
            var parser = new QueryParser("");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagNextNode()
        {
            var parser = new QueryParser(":tag");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(2, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void TagOpenParanthesisNextNode()
        {
            var parser = new QueryParser("(:tag");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void TagAndNextNode()
        {
            var parser = new QueryParser("(:tag and ");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void NotNextNode()
        {
            var parser = new QueryParser("not ");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void OpenParentesisNotNextNode()
        {
            var parser = new QueryParser("(not ");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagComplexNextNode()
        {
            var parser = new QueryParser("(:test or ((:tag or :bla and :fun) and :x");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(3, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void CompleteExpressionNextNode()
        {
            var parser = new QueryParser("(:tag and :fun)");
            var ir = parser.Parse();
            var next = parser.NextToken();

            Assert.Equal(2, next.Count());
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void IllegalNextNode()
        {
            var parser = new QueryParser(")");
            var ir = parser.Parse();
            var next = parser.NextToken();

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
            var parser = new QueryParser(":tax:2016 || :cool && :audio:original");
            var visitor = new DB.SearchQueryVisitor(new MockDatabase(), new MockTagParser());

            var ir = parser.Parse();

            ir.Accept(visitor);

            Assert.Equal(SelectStatement, visitor.Result);
        }
    }
}
