using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using Console = Apophis.CLI.Console;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Commands;

/// <summary>
/// The overview of all commands, and the generated help of a single command. The commands are lazy, because this command is one of them.
/// </summary>
[UsedImplicitly]
internal class HelpCommand(
    Console console,
    Lazy<IEnumerable<ICommand>> commands,
    Lazy<IEnumerable<IDeclaredCommand>> declaredCommands,
    IEnumerable<IOption> options,
    Lazy<CommandLine> commandLine,
    IAutoTagHandler autoTagHandler) : IDeclaredCommand
{
    private static readonly ArgumentDefinition CommandArgument = new() { Name = "command", Description = "The command, or one of its aliases, to show the help of.", Arity = Arity.ZeroOrOne };

    private static readonly OptionDefinition Verbose = new() { Name = "verbose", ShortName = "v", Description = "Describes the tag variables in the overview." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "help";
    public IReadOnlyList<string> Aliases => ["?"];
    public string Description => "This is the help command, it shows a list of the available commands.";
    public string Details => "Without a command, it shows the overview of all commands. With a command, it shows the help of that command, the same as '<command> --help'.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [CommandArgument];
    public IReadOnlyList<OptionDefinition> Options => [Verbose];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(CommandArgument).Match(
            none: () => PrintOverview(input.Has(Verbose)),
            some: PrintCommandHelp));

    private static (CommandGrouping Grouping, string Name, string Aliases, string Description) HelpRow(ICommand command)
        => (command.CommandGrouping, command.LongCommand, command.ShortCommand.GetOrElse(string.Empty), command.ShortHelp);

    private static (CommandGrouping Grouping, string Name, string Aliases, string Description) HelpRow(IDeclaredCommand command)
        => (command.CommandGrouping, command.Name, string.Join(", ", command.Aliases), command.Description);

    private static IEnumerable<string> Names(ICommand command)
        => [command.LongCommand, .. command.ShortCommand.ToEnumerable()];

    private static IEnumerable<string> Names(IDeclaredCommand command)
        => [command.Name, .. command.Aliases];

    private static string FormatOption(Option<string> option, string optionPrefix)
        => option.Match(none: string.Empty, some: name => $"{optionPrefix}{name}");

    private static ConsoleTable HelpTable()
    {
        var table = new ConsoleTable(3);

        table.Columns[0].PaddingLeft = 2;

        return table;
    }

    /// <summary>The help of every command, old-style or declared, is generated from what the command line knows about it.</summary>
    private bool PrintCommandHelp(string name)
        => commands.Value.SelectMany(Names).Concat(declaredCommands.Value.SelectMany(Names)).Contains(name)
            ? commandLine.Value.Run([name, "--help"]) == ExitCode.Success
            : UnknownCommand(name);

    private bool UnknownCommand(string name)
    {
        console.Writer.WriteLine($"Unknown command '{name}', '{console.Application.Name} help' lists all commands.");

        return false;
    }

    private bool PrintOverview(bool verbose)
    {
        console.Writer.WriteLine($"{console.Application.Name} <command> [<arguments>] [<options>]:");
        PrintHelpCommands([.. commands.Value.Select(HelpRow), .. declaredCommands.Value.Select(HelpRow)]);
        PrintHelpOptions();
        PrintTagVariables(verbose);
        PrintHelpExamples();

        return true;
    }

    private void PrintHelpCommands(IReadOnlyList<(CommandGrouping Grouping, string Name, string Aliases, string Description)> rows)
    {
        console.Writer.WriteLine();
        console.Writer.WriteLine("Commands");
        console.Writer.WriteLine();

        var table = HelpTable();
        foreach (var commandGrouping in Enum.GetValues<CommandGrouping>())
        {
            foreach (var row in rows.Where(row => row.Grouping == commandGrouping))
            {
                table.Append(row.Name, row.Aliases, row.Description);
            }

            table.AppendSeparator();
        }

        table.WriteTo(console.Writer);
    }

    /// <summary>Only plugins with old-style commands still contribute global options; the declared commands show their own options.</summary>
    private void PrintHelpOptions()
    {
        var globalOptions = options.ToList();
        if (globalOptions.Count > 0)
        {
            console.Writer.WriteLine();
            console.Writer.WriteLine("Options of the old-style plugin commands");
            console.Writer.WriteLine();

            var table = HelpTable();
            foreach (var option in globalOptions)
            {
                table.Append(FormatOption(option.ShortOption, "-"), FormatOption(option.LongOption, "--"), option.ShortHelp);
            }

            table.AppendSeparator();
            table.WriteTo(console.Writer);
        }
    }

    private void PrintTagVariables(bool verbose)
    {
        console.Writer.WriteLine("Possible Tag Variables:");
        console.Writer.WriteLine();
        foreach (var tag in autoTagHandler.AutoTags.OrderBy(tag => tag.AutoTagKey))
        {
            console.Writer.WriteLine($"* {tag.HumanReadableAutoTagsKey}");
            if (verbose)
            {
                console.Writer.WriteLine($"=>  {tag.Description}");
                console.Writer.WriteLine();
            }
        }
    }

    private void PrintHelpExamples()
    {
        console.Writer.WriteLine();
        console.Writer.WriteLine("Examples:");
        console.Writer.WriteLine();
        console.Writer.WriteLine($"  {console.Application.Name} find-files (:tax:2018 or :tax:2017) and :bank");
        console.Writer.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
        console.Writer.WriteLine();
        console.Writer.WriteLine($"  {console.Application.Name} tag-files :Files:{{FileType.Category}}:{{Camera.Make}} :Taken:{{Camera.Taken.Year}} *");
        console.Writer.WriteLine("    This will tag all files in the current directory, the possible automatic tags depend on your plugins.");
    }
}
