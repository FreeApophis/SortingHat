using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.Test.Mock;
using Xunit;

namespace SortingHat.Test
{
    public class TagTest
    {
        private readonly ITag _tag;
        private readonly TagParser _tagParser;

        public TagTest()
        {
            _tag = MockTagStore.Create();
            _tagParser = new TagParser((name, parent) => new Tag(_tag, name, parent));
        }

        [Fact]
        public void GivenTwoTagsWithTheSameValueTheyTheyShouldBeEqual()
        {

            var tag1 = new Tag(_tag, "2018", new Tag(_tag, "created"));
            var tag2 = new Tag(_tag, "2018", new Tag(_tag, "created"));
            var tag3 = new Tag(_tag, "2018");

            Assert.Equal(tag1, tag2);
            Assert.NotEqual(tag1, tag3);
        }

        [Fact]
        public void GivenAStringRepresentingATagThenTagParseShouldReturnATagWithTheRightValue()
        {
            var referenceTag = new Tag(_tag, "tag");
            var parsedTag = _tagParser.Parse(":tag");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseSimpleTagMissingColon()
        {
            var referenceTag = null as Tag;
            var parsedTag = _tagParser.Parse("tag");

            Assert.Equal(referenceTag, parsedTag);
        }


        [Fact]
        public void ParseTag()
        {
            var referenceTag = new Tag(_tag, "child", new Tag(_tag, "father", new Tag(_tag, "grandfather")));
            var parsedTag = _tagParser.Parse(":grandfather:father:child");

            Assert.Equal(referenceTag, parsedTag);
        }


        [Fact]
        public void ParseEmptyTag()
        {
            var referenceTag = null as Tag;
            var parsedTag = _tagParser.Parse(":");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseTagMissingColon()
        {
            var referenceTag = null as Tag;
            var parsedTag = _tagParser.Parse("grandfather:father:child");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseTagWhitespace()
        {
            var referenceTag = new Tag(_tag, "child with whitespace", new Tag(_tag, "father", new Tag(_tag, "grand father")));
            var parsedTag = _tagParser.Parse(":grand father:father:child with whitespace");

            Assert.Equal(referenceTag, parsedTag);
        }

        [Fact]
        public void ParseMultipleColon()
        {
            var parsedTag = _tagParser.Parse("::first:::last");

            Assert.NotNull(parsedTag);

            // ReSharper disable once ConstantConditionalAccessQualifier
            Assert.Equal(":first:last", parsedTag?.FullName);
        }
    }
}
