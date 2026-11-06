using Microsoft.Extensions.Logging;

namespace SortingHat.UI;

internal sealed class WindowLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LoggerWindow();
    }
}
