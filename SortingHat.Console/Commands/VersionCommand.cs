using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
[method: UsedImplicitly]
internal class VersionCommand(IAnsiConsole console) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "version";
    public IReadOnlyList<string> Aliases => ["v"];
    public string Description => "Shows the version of hat.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var version = Assembly.GetExecutingAssembly().GetName();

        console.WriteLine($"{nameof(SortingHat)} {version.Version}");
        console.WriteLine();
        console.Write(new Grid()
            .AddColumn(new GridColumn().RightAligned())
            .AddColumn()
            .AddRow(new Text("Culture:"), new Text(Option.FromNullable(version.CultureName).Where(name => name is not "").GetOrElse("neutral")))
            .AddRow(new Text("Current Culture:"), new Text(CultureInfo.CurrentCulture.Name)));

        return Task.FromResult(true);
    }
}
