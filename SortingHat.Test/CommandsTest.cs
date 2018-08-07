using SortingHat.API.Models;
using SortingHat.CLI.Commands;
using SortingHat.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SortingHat.Test
{
    public class CommandsTest
    {
        [Fact]
        public void CreateAndListTagsTest()
        {
            var service = new MockService();
            var addTag = new AddTagCommand(service);

            List<string> taxPeriods = new List<string>() { "tags", "add", ":tax_period:2016", ":tax_period:2017", ":tax_period:2018", ":tax_period:2019" };
            List<string> movieRating = new List<string>() { "tags", "add", ":movie:bad", ":movie:average", ":movie:good", ":movie:great" };

            addTag.Execute(taxPeriods);
            addTag.Execute(taxPeriods);


            var command = new ListTagsCommand(service);
            command.Execute(new[] { "tag", "list" });
        }
    }
}
