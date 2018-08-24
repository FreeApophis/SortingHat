using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace SortingHat.UI
{
    internal class LogItem
    {
        public LogLevel LogLevel { get; }

        public LogItem(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

    }

    /// <summary>
    /// Interaction logic for LoggerWindow.xaml
    /// </summary>
    public partial class LoggerWindow : Window, ILogger
    {
        public LoggerWindow()
        {
            InitializeComponent();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogView.Items.Add(new LogItem(logLevel));
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
