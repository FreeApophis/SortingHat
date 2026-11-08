using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI.Commands;

/// <summary>
/// The overview of all commands, and the generated help of a single command. The command names are lazy, because this command is one of the commands.
/// </summary>
[UsedImplicitly]
internal class HelpCommand(
    IConsoleWriter consoleWriter,
    ApplicationName application,
    IAnsiConsole console,
    Lazy<CommandNames> commandNames,
    Lazy<CommandLine> commandLine,
    IAutoTagHandler autoTagHandler) : IDeclaredCommand
{
    private static readonly ArgumentDefinition CommandArgument = new() { Name = "command", Description = "The command, or one of its aliases, to show the help of.", Arity = Arity.ZeroOrOne };

    private static readonly OptionDefinition Verbose = new() { Name = "verbose", ShortName = "v", Description = "Describes the tag variables in the overview." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "help";
    public IReadOnlyList<string> Aliases => ["?"];
    public string Description => "Lists the available commands, or shows the help of one command.";
    public string Details => "Without a command, it shows the overview of all commands. With a command, it shows the help of that command, the same as '<command> --help'.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [CommandArgument];
    public IReadOnlyList<OptionDefinition> Options => [Verbose];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(CommandArgument).Match(
            none: () => PrintOverview(input.Has(Verbose)),
            some: PrintCommandHelp));

    private static (CommandGrouping Grouping, string Name, string Aliases, string Description) HelpRow(OfferedCommand offered)
        => (offered.Command.CommandGrouping, offered.Command.Name, string.Join(", ", offered.Aliases), offered.Command.Description);

    private static IEnumerable<string> Names(OfferedCommand offered)
        => [offered.Command.Name, .. offered.Aliases];

    /// <summary>The help of every command is generated from what the command line knows about it.</summary>
    private bool PrintCommandHelp(string name)
        => commandNames.Value.Offered.SelectMany(Names).Contains(name)
            ? commandLine.Value.Run([name, "--help"]) == ExitCode.Success
            : UnknownCommand(name);

    private bool UnknownCommand(string name)
    {
        consoleWriter.WriteLine($"Unknown command '{name}', '{application.Name} help' lists all commands.");

        return false;
    }

    private bool PrintOverview(bool verbose)
    {
        consoleWriter.WriteLine($"{application.Name} <command> [<arguments>] [<options>]:");
        PrintHelpCommands([.. commandNames.Value.Offered.Select(HelpRow)]);
        PrintTagVariables(verbose);
        PrintHelpExamples();

        return true;
    }

    private void PrintHelpCommands(IReadOnlyList<(CommandGrouping Grouping, string Name, string Aliases, string Description)> rows)
    {
        consoleWriter.WriteLine();

        var table = LightTable.Create(LightTable.Column("Command"), LightTable.Column("Aliases"), LightTable.Column("Description"));
        foreach (var (group, index) in Enum.GetValues<CommandGrouping>().Select(group => rows.Where(row => row.Grouping == group).ToList()).Where(group => group is not []).Select((group, index) => (group, index)))
        {
            if (index > 0)
            {
                table.AddEmptyRow();
            }

            group.ForEach(row => table.AddCells(row.Name, row.Aliases, row.Description));
        }

        console.Write(table);
        consoleWriter.WriteLine();
    }

    private void PrintTagVariables(bool verbose)
    {
        consoleWriter.WriteLine("Possible Tag Variables:");
        consoleWriter.WriteLine();
        foreach (var tag in autoTagHandler.AutoTags.OrderBy(tag => tag.Name, System.StringComparer.Ordinal))
        {
            consoleWriter.WriteLine($"* {tag.Parameter.Usage(tag.Name)}");
            if (verbose)
            {
                consoleWriter.WriteLine($"=>  {tag.Description}");
                consoleWriter.WriteLine();
            }
        }
    }

    private void PrintHelpExamples()
    {
        consoleWriter.WriteLine();
        consoleWriter.WriteLine("Examples:");
        consoleWriter.WriteLine();
        consoleWriter.WriteLine($"  {application.Name} find-files (:tax:2018 or :tax:2017) and :bank");
        consoleWriter.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
        consoleWriter.WriteLine();
        consoleWriter.WriteLine($"  {application.Name} tag-files :Files:{{FileType.Category}}:{{Camera.Make}} :Taken:{{Camera.Taken.Year}} *");
        consoleWriter.WriteLine("    This will tag all files in the current directory, the possible automatic tags depend on your plugins.");
    }
}
