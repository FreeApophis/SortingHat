using SortingHat.CLI.Commands;
using System.Collections.Generic;
using SortingHat.API.Models;
using Moq;
using SortingHat.API.DI;
using Xunit;
using SortingHat.ConsoleWriter;
using System.Linq;

namespace SortingHat.Test
{
    public class CommandsTest
    {
        [Fact]
        public void CreateAndListTagsTest()
        {
            var db = MockDatabase.Create();
            var tagParser = new TagParser((name, parent) => new Tag(db, name, parent));
            ICommand addTag = new AddTagCommand(tagParser);

            List<string> taxPeriods = new List<string> { ":tax_period:2016", ":tax_period:2017", ":tax_period:2018", ":tax_period:2019" };
            List<string> movieRating = new List<string> { ":movie:bad", ":movie:average", ":movie:good", ":movie:great" };

            var mockOptions = new Mock<IOptions>();

            mockOptions.Setup(o => o.HasOption(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            addTag.Execute(taxPeriods, mockOptions.Object);
            addTag.Execute(movieRating, mockOptions.Object);

            Assert.Collection(db.MockTag.Tags,
                item => Assert.Equal("2016", item.Name),
                item => Assert.Equal("tax_period", item.Parent?.Name),
                item => Assert.Equal(taxPeriods[2], item.FullName),
                item => Assert.Equal(taxPeriods[3], item.FullName),
                item => Assert.Equal("bad", item.Name),
                item => Assert.Equal("movie", item.Parent?.Name),
                item => Assert.Equal(movieRating[2], item.FullName),
                item => Assert.Equal(movieRating[3], item.FullName)
                );

            var console = new MemoryConsoleWriter();
            ICommand listTags = new ListTagsCommand(db, console);

            listTags.Execute(Enumerable.Empty<string>(), mockOptions.Object);

            Assert.Collection(console.Lines,
                line => Assert.Equal("", line)
                );

        }
    }
}
