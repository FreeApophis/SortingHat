using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        [Fact]
        public void EmptySearch()
        {
            var parser = new QueryParser("");
            var parseTree = parser.Parse();

            Assert.Null(parseTree);
        }

        [Fact]
        public void SingleTag()
        {
            var parser = new QueryParser(":tax:2018");
            var parseTree = parser.Parse();

            Assert.IsType<TagNode>(parseTree);

            Assert.Equal(":tax:2018", (parseTree as TagNode).Tag);
        }

        [Fact]
        public void SimpleQuery()
        {
            var parser = new QueryParser(":test or true and (:movie or not :blue)");
            var visitor = new ToStringVisitor();

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);
        }

        [Fact]
        public void SQLQuery()
        {
            var parser = new QueryParser(":test && :cool");
            var visitor = new DB.SearchQueryVisitor();

            var ir = parser.Parse();

            ir.Accept(visitor);
            //Assert.Equal("", visitor.Result);
        }
    }
}
