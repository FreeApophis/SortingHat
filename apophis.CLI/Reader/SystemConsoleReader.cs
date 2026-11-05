using Funcky.Extensions;
using Funcky.Monads;

namespace apophis.CLI.Reader
{
    public class SystemConsoleReader : IConsoleReader

    {
        public Option<int> ReadInt()
        {
            return ReadLine().ParseInt32OrNone();
        }

        public string ReadLine()
        {
            // ReadLine returns null at the end of the input stream
            return System.Console.ReadLine() ?? string.Empty;
        }
    }
}
