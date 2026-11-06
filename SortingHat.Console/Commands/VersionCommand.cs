using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
[method: UsedImplicitly]
internal class VersionCommand(IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "version";
    public IReadOnlyList<string> Aliases => ["v"];
    public string Description => "Shows the current Version of this program";

    public string Details
        => """
           This command shows the current version of the program, but probably you wanted to see this in ASCII Art anyway:

              _____            _   _             _    _       _     __   ___
             / ____|          | | (_)           | |  | |     | |   /_ | / _ \
            | (___   ___  _ __| |_ _ _ __   __ _| |__| | __ _| |_   | || | | |
             \___ \ / _ \| '__| __| | '_ \ / _` |  __  |/ _` | __|  | || | | |
             ____) | (_) | |  | |_| | | | | (_| | |  | | (_| | |_   | || |_| |
            |_____/ \___/|_|   \__|_|_| |_|\__, |_|  |_|\__,_|\__|  |_(_)___/
                                            __/ |
                                           |___/
           """;

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var version = Assembly.GetExecutingAssembly().GetName();

        consoleWriter.WriteLine($"{nameof(SortingHat)} {version.Version}");
        consoleWriter.WriteLine();

        var table = new ConsoleTable(2);

        table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;

        table.Append("Culture:", version.CultureName ?? "<NULL>");
        table.Append("Current Culture:", CultureInfo.CurrentCulture);

        table.WriteTo(consoleWriter);

        return Task.FromResult(true);
    }
}
