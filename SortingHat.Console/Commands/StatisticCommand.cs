using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class StatisticCommand(IProjectDatabase db, IAnsiConsole console) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "statistics";
    public IReadOnlyList<string> Aliases => ["stat"];
    public string Description => "Shows statistics about the active project.";
    public string Details => "Counts the files, paths, names, tags and tag assignments of the active project.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var table = LightTable.Create(LightTable.Column("Statistic"), LightTable.Column("Count").RightAligned());
        db.GetStatistics().ForEach(statistic => table.AddCells(statistic.Key, $"{statistic.Value}"));

        console.Write(table);

        return Task.FromResult(true);
    }
}
