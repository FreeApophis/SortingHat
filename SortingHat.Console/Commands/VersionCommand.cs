using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
[method: UsedImplicitly]
internal class VersionCommand(IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "version";
    public Option<string> ShortCommand => Option.Some("v");
    public string ShortHelp => "Shows the current Version of this program";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var version = GetVersion();

        consoleWriter.WriteLine($"{nameof(SortingHat)} {version.Version}");
        consoleWriter.WriteLine();

        var table = new ConsoleTable(2);

        table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;

        table.Append("Culture:", version.CultureName ?? "<NULL>");
        table.Append("Current Culture:", CultureInfo.CurrentCulture);

        table.WriteTo(consoleWriter);
        return true;
    }

    private static AssemblyName GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName();
    }
}
