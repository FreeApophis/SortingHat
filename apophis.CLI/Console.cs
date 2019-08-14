using apophis.CLI.Reader;
using apophis.CLI.Writer;

namespace apophis.CLI
{
    public class Console
    {
        public Console(IConsoleReader consoleReader, IConsoleWriter consoleWriter, IConsoleApplicationInformationProvider consoleApplicationInformationProvider)
        {
            Reader = consoleReader;
            Writer = consoleWriter;
            Application = consoleApplicationInformationProvider;
        }

        public IConsoleReader Reader { get; }
        public IConsoleWriter Writer { get; }

        public IConsoleApplicationInformationProvider Application { get; }
    }
}
