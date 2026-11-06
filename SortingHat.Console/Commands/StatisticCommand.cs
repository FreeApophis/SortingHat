using System.Collections.Generic;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class StatisticCommand(IMainDatabase db, IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "statistics";
    public Option<string> ShortCommand => Option.Some("stat");
    public string ShortHelp => "Shows global statistics";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var table = StatisticsTable();

        foreach (var (key, value) in db.GetStatistics())
        {
            table.Append(key, "=", value);
        }

        consoleWriter.WriteLine("Statistics:");
        consoleWriter.WriteLine();
        table.WriteTo(consoleWriter);

        return true;
    }

    private static ConsoleTable StatisticsTable()
    {
        var table = new ConsoleTable(3);

        table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;
        table.Columns[0].PaddingLeft = 2;

        return table;
    }
}
