using SortingHat.CLI.Commands;
using System.Collections.Generic;
using Xunit;

namespace SortingHat.Test
{
    public class CommandsTest
    {
        [Fact]
        public void CreateAndListTagsTest()
        {
            var tagParser = new MockTagParser();
            var addTag = new AddTagCommand(tagParser);

            List<string> taxPeriods = new List<string>() { "tags", "add", ":tax_period:2016", ":tax_period:2017", ":tax_period:2018", ":tax_period:2019" };
            List<string> movieRating = new List<string>() { "tags", "add", ":movie:bad", ":movie:average", ":movie:good", ":movie:great" };

            addTag.Execute(taxPeriods);

            addTag.Execute(movieRating);
        }
    }
}
