using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class StatisticCommand(IProjectDatabase db, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "statistics";
    public IReadOnlyList<string> Aliases => ["stat"];
    public string Description => "Shows global statistics";
    public string Details => "Counts the files, paths, names, tags and tag assignments of the active project.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var table = StatisticsTable();

        foreach (var (key, value) in db.GetStatistics())
        {
            table.Append(key, "=", value);
        }

        consoleWriter.WriteLine("Statistics:");
        consoleWriter.WriteLine();
        table.WriteTo(consoleWriter);

        return Task.FromResult(true);
    }

    private static ConsoleTable StatisticsTable()
    {
        var table = new ConsoleTable(3);

        table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;
        table.Columns[0].PaddingLeft = 2;

        return table;
    }
}
