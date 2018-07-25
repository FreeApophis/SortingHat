using SortingHat.API.Interfaces;
using System.Linq;
using Xunit;

namespace SortingHat.Test
{
    public class LoggerTest
    {
        const string message = "This is a message!";

        [Fact]
        public void BasicLoggerTest()
        {
            var service = new MockService();

            service.Logger.Log(LogCategory.Trace, message);

            var mockLogger = service.Logger as MockLogger;

            Assert.NotNull(mockLogger);
            Assert.Single(mockLogger.Lines);

            var line = mockLogger.Lines.First();

            Assert.Equal(LogCategory.Trace, line.LogCategory);
            Assert.Equal(message, line.Message);
            Assert.EndsWith("LoggerTest.cs", line.File);
            Assert.Equal("BasicLoggerTest", line.Caller);
            Assert.Equal(16, line.Line);
        }
    }
}
