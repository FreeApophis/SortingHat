using SortingHat.CLI.Commands;
using Xunit;

namespace SortingHat.Test
{
    public class CommandsTest
    {
        [Fact]
        public void BasicLoggerTest()
        {
            var service = new MockService();
            var command = new ListTagsCommand(service);

        }
    }
}
