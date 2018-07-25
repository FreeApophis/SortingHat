
using SortingHat.API.Models;
using Xunit;

namespace SortingHat.Test
{
    public class TagTest
    {
        [Fact]
        public void TagEquivalence()
        {
            var tag1 = new Tag("2018", new Tag("created"));
            var tag2 = new Tag("2018", new Tag("created"));
            var tag3 = new Tag("2018");

            Assert.Equal(tag1, tag2);
            Assert.NotEqual(tag1, tag3);
        }
    }
}
