using Apophis.CLI.Reader;
using Apophis.CLI.Writer;

namespace Apophis.CLI;

public class Console(IConsoleReader consoleReader, IConsoleWriter consoleWriter, IConsoleApplicationInformationProvider consoleApplicationInformationProvider)
{
    public IConsoleReader Reader { get; } = consoleReader;
    public IConsoleWriter Writer { get; } = consoleWriter;

    public IConsoleApplicationInformationProvider Application { get; } = consoleApplicationInformationProvider;
}
