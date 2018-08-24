using System;
using Microsoft.Extensions.Logging;

namespace SortingHat.UI
{
    class WindowLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerWindow();
        }
    }
}
