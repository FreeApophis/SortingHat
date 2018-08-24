
using SortingHat.API.DI;
using SortingHat.API.Models;
using Xunit;

namespace SortingHat.Test
{
    public class TagTest
    {
        private readonly IDatabase _db = new MockDatabase();
        private readonly TagParser _tagParser = new TagParser((name, parent) => new Tag(new MockDatabase(), name, parent));

        [Fact]
        public void TagEquivalence()
        {

            var tag1 = new Tag(_db, "2018", new Tag(_db, "created"));
            var tag2 = new Tag(_db, "2018", new Tag(_db, "created"));
            var tag3 = new Tag(_db, "2018");

            Assert.Equal(tag1, tag2);
            Assert.NotEqual(tag1, tag3);
        }

        [Fact]
        public void ParseSimpleTag()
        {
            var referenceTag = new Tag(_db, "tag");
            Tag parsedTag = _tagParser.Parse(":tag");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseSimpleTagMissingColon()
        {
            Tag referenceTag = null;
            Tag parsedTag = _tagParser.Parse("tag");

            Assert.Equal(referenceTag, parsedTag);
        }


        [Fact]
        public void ParseTag()
        {
            var referenceTag = new Tag(_db, "child", new Tag(_db, "father", new Tag(_db, "grandfather")));
            Tag parsedTag = _tagParser.Parse(":grandfather:father:child");

            Assert.Equal(referenceTag, parsedTag);
        }


        [Fact]
        public void ParseEmptyTag()
        {
            Tag referenceTag = null;
            Tag parsedTag = _tagParser.Parse(":");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseTagMissingColon()
        {
            Tag referenceTag = null;
            Tag parsedTag = _tagParser.Parse("grandfather:father:child");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseTagWhitespace()
        {
            Tag referenceTag = null;
            Tag parsedTag = _tagParser.Parse("grand father:father:child with whitespace");

            Assert.Equal(referenceTag, parsedTag);
        }
    }
}
