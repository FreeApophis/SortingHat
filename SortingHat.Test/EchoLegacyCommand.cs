using System.Collections.Generic;
using Apophis.CLI.Writer;
using Funcky.Monads;
using SortingHat.API.DI;

namespace SortingHat.Test;

/// <summary>
/// A command in the older <see cref="ICommand"/> style, like a plugin that has not moved to <see cref="IDeclaredCommand"/> yet.
/// </summary>
internal sealed class EchoLegacyCommand(IConsoleWriter consoleWriter) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public string LongCommand => "echo";

    public Option<string> ShortCommand => Option.Some("e");

    public string ShortHelp => "Echoes its arguments.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        consoleWriter.WriteLine(string.Join(" ", arguments));

        return true;
    }
}
