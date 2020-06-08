using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using System.Linq;
using apophis.Lexer;
using SortingHat.API.Parser.OperatorType;
using SortingHat.API.Parser.Token;
using SortingHat.Test.Mock;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        private readonly Parser _parser = Parser.Create();

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

            AcceptVisitor(root, visitor);

            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);
            Assert.False(_parser.IllegalExpression);
        }

        private void AcceptVisitor(IParseNode? root, INodeVisitor visitor)
        {
            Assert.NotNull(root);

            // ReSharper understands, this cannot be null, but the VisualStudio Code analyzer does not...
            // ReSharper disable once ConstantConditionalAccessQualifier
            root?.Accept(visitor);
        }

        [Fact]
        public void PrecedenceTest()
        {
            var root = _parser.Parse(":A || :B && :C");
            var visitor = new ToStringVisitor(new LogicalOperatorType());

            AcceptVisitor(root, visitor);
            Assert.Equal("(:A ∨ (:B ∧ :C))", visitor.Result);
        }

        [Fact]
        public void OrAssociativityTest()
        {
            var root = _parser.Parse(":A || :B || :C || :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            AcceptVisitor(root, visitor);
            Assert.Equal("(((:A || :B) || :C) || :D)", visitor.Result);
        }

        [Fact]
        public void AndAssociativityTest()
        {
            var root = _parser.Parse(":A && :B && :C && :D");
            var visitor = new ToStringVisitor(new ProgrammingOperatorType());

            AcceptVisitor(root, visitor);
            Assert.Equal("(((:A && :B) && :C) && :D)", visitor.Result);
        }

        [Fact]
        public void MultipleNotTest()
        {
            var root = _parser.Parse("!!!:A && !!:B");
            var visitor = new ToStringVisitor(new TextOperatorType());

            AcceptVisitor(root, visitor);
            Assert.Equal("(not(not(not(:A))) and not(not(:B)))", visitor.Result);
        }

        [Fact]
        public void EmptyStringNextNode()
        {
            var _ = _parser.Parse("");
            var next = _parser.NextToken().ToList();

            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagNextNode()
        {
            var _ = _parser.Parse(":tag");
            var next = _parser.NextToken().ToList();

            Assert.Equal(2, next.Count);
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void TagOpenParanthesisNextNode()
        {
            var _ = _parser.Parse("(:tag");
            var next = _parser.NextToken().ToList();

            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void TagAndNextNode()
        {
            var _ = _parser.Parse("(:tag and ");
            var next = _parser.NextToken().ToList();
            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void NotNextNode()
        {
            var _ = _parser.Parse("not ");
            var next = _parser.NextToken().ToList();

            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void OpenParentesisNotNextNode()
        {
            var _ = _parser.Parse("(not ");
            var next = _parser.NextToken().ToList();

            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is TagToken);
            Assert.Contains(next, node => node is NotToken);
            Assert.Contains(next, node => node is OpenParenthesisToken);
        }

        [Fact]
        public void TagComplexNextNode()
        {
            var _ = _parser.Parse("(:test or ((:tag or :bla and :fun) and :x");
            var next = _parser.NextToken().ToList();

            Assert.Equal(3, next.Count);
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
            Assert.Contains(next, node => node is ClosedParenthesisToken);
        }

        [Fact]
        public void CompleteExpressionNextNode()
        {
            var _ = _parser.Parse("(:tag and :fun)");
            var next = _parser.NextToken().ToList();

            Assert.Equal(2, next.Count);
            Assert.Contains(next, node => node is AndToken);
            Assert.Contains(next, node => node is OrToken);
        }

        [Fact]
        public void IllegalNextNode()
        {
            var _ = _parser.Parse(")");
            var next = _parser.NextToken();

            Assert.Empty(next);
        }

        private const string SelectStatement = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path
FROM Files
JOIN FilePaths ON FilePaths.FileId = Files.Id
WHERE (0 OR (0 AND 0))
GROUP BY FilePaths.Id";

        [Fact]
        public void SQLQuery()
        {
            var root = _parser.Parse(":tax:2016 || :cool && :audio:original");
            //var visitor = new DB.SearchQueryVisitor(MockProjectDatabase.Create(), MockTagStore.Create(), new MockTagParser());

            //AcceptVisitor(root, visitor);
            //Assert.Equal(SelectStatement, visitor.Result);
        }
    }
}
