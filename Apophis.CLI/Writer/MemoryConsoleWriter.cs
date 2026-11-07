using System.Collections.Generic;
using System.Threading;

namespace Apophis.CLI.Writer;

public class MemoryConsoleWriter : IConsoleWriter
{
    private readonly List<string> _lines = [];
    private readonly Lock _lock = new();

    public MemoryConsoleWriter()
    {
    }

    public List<string> Lines => _lines;

    /// <summary>Thread-safe like the real console, files are hashed in parallel.</summary>
    public void WriteLine(string line)
    {
        lock (_lock)
        {
            _lines.Add(line);
        }
    }

    public void WriteLine()
        => WriteLine(string.Empty);
}
