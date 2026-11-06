using Funcky.Extensions;
using Funcky.Monads;

namespace Apophis.CLI.Reader;

public class SystemConsoleReader : IConsoleReader
{
    public Option<int> ReadInt()
    {
        return int.ParseOrNone(ReadLine());
    }

    public string ReadLine()
    {
        // ReadLine returns null at the end of the input stream
        return System.Console.ReadLine() ?? string.Empty;
    }
}
