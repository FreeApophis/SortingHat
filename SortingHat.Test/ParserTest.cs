using SortingHat.API.Parser;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        [Fact]
        public void SimpleQuery()
        {
            var parser = new QueryParser(":test or true and ( :movie or not :blue )");
            var visitor = new ToStringVisitor();

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);

        }
    }
}
