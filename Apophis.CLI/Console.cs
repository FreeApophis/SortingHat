using Apophis.CLI.Writer;

namespace Apophis.CLI;

public class Console(IConsoleWriter consoleWriter, IConsoleApplicationInformationProvider consoleApplicationInformationProvider)
{
    public IConsoleWriter Writer { get; } = consoleWriter;

    public IConsoleApplicationInformationProvider Application { get; } = consoleApplicationInformationProvider;
}
