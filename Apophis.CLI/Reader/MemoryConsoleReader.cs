using System.Collections.Generic;
using Funcky.Extensions;
using Funcky.Monads;

namespace Apophis.CLI.Reader;

/// <summary>
/// Reads the given lines, as if the user had typed them, and then behaves like the end of the input stream.
/// </summary>
public class MemoryConsoleReader : IConsoleReader
{
    private readonly Queue<string> _lines = [];

    public void Type(params IEnumerable<string> lines)
        => lines.ForEach(_lines.Enqueue);

    public Option<int> ReadInt()
        => int.ParseOrNone(ReadLine());

    public string ReadLine()
        => _lines.DequeueOrNone().GetOrElse(string.Empty);
}
