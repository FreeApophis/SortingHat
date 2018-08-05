using SortingHat.API.Parser;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        [Fact]
        public void TagEquivalence()
        {
            var parser = new QueryParser(":test or true and (:movie or not :blue)");
            var visitor = new ToStringVisitor();

            var ir = parser.Parse();

            visitor.Visit(ir);
            Assert.Equal("", visitor.Result);

        }
    }
}
