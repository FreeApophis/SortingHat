using apophis.CLI.Reader;
using apophis.CLI.Writer;

namespace apophis.CLI
{
    public class Console
    {
        public Console(IConsoleReader consoleReader, IConsoleWriter consoleWriter)
        {
            Reader = consoleReader;
            Writer = consoleWriter;
        }

        public IConsoleReader Reader { get; }
        public IConsoleWriter Writer { get; }
    }
}
