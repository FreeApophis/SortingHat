using System.Threading;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>
/// The console of plugins: plain lines, written as they are to the output of the Spectre console.
/// Not as markup, so '[' in a path stays, and not wrapped, so a long path stays on one line.
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
            console.Profile.Out.Writer.WriteLine(line);
        }
    }

    public void WriteLine()
        => WriteLine(string.Empty);
}
