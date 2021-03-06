﻿using System.Collections.Generic;
using System.Linq;
using apophis.CLI.Writer;
using Moq;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CLI.Commands.Tags;
using SortingHat.Test.Mock;
using Xunit;

namespace SortingHat.Test.Commands
{
    public class TagCommandsTest
    {
        [Fact]
        public void GivenAListOfTagsToTheCreateCommandThenTheListCommandListsThemCorrectly()
        {
            var tag = MockTagStore.Create();

            var tagParser = new TagParser((name, parent) => new Tag(tag, name, parent));
            ICommand addTag = new AddTagCommand(tagParser);

            List<string> taxPeriods = new List<string> { ":tax_period:2016", ":tax_period:2017", ":tax_period:2018", ":tax_period:2019" };
            List<string> movieRating = new List<string> { ":movie:bad", ":movie:average", ":movie:good", ":movie:great" };

            var mockOptions = new Mock<IOptionParser>();

            addTag.Execute(taxPeriods, mockOptions.Object);
            addTag.Execute(movieRating, mockOptions.Object);

            Assert.Collection(tag.Tags,
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
            ICommand listTags = new ListTagsCommand(tag, console);

            listTags.Execute(Enumerable.Empty<string>(), mockOptions.Object);

            Assert.Collection(console.Lines,
                line => Assert.Equal("Used tags: ", line),
                line => Assert.Equal("* :tax_period:2016 (0) ", line),
                line => Assert.Equal("* :tax_period:2017 (0) ", line),
                line => Assert.Equal("* :tax_period:2018 (0) ", line),
                line => Assert.Equal("* :tax_period:2019 (0) ", line),
                line => Assert.Equal("* :movie:bad       (0) ", line),
                line => Assert.Equal("* :movie:average   (0) ", line),
                line => Assert.Equal("* :movie:good      (0) ", line),
                line => Assert.Equal("* :movie:great     (0) ", line)
                );

        }
    }
}
