using System.Threading;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>
/// The console of plugins: plain lines, written to the Spectre console as text.
/// Not as markup, so '[' in a path stays. Through Spectre, so a line written while a progress bar shows goes above the bar.
/// </summary>
[UsedImplicitly]
internal sealed class PlainLineWriter(IAnsiConsole console) : IConsoleWriter
{
    private readonly Lock _lock = new();

    /// <summary>Thread-safe like the real console, files are hashed in parallel.</summary>
    public void WriteLine(string line)
    {
        lock (_lock)
        {
            console.WriteLine(line);
        }
    }

    public void WriteLine()
        => WriteLine(string.Empty);
}
