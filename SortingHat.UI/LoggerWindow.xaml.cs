using System;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace SortingHat.UI;

public partial class LoggerWindow : Window, ILogger
{
    public LoggerWindow()
    {
        InitializeComponent();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogView.Items.Add(new LogItem(logLevel));
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        throw new NotImplementedException();
    }
}

internal class LogItem
{
    public LogItem(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }

    public LogLevel LogLevel { get; }
}
