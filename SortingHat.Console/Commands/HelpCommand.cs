using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apophis.CLI;
using Autofac;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.CLI.Options;
using Console = Apophis.CLI.Console;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class HelpCommand(ILogger<HelpCommand> logger, Console console, IComponentContext container, IConsoleApplicationInformationProvider consoleApplicationInformation) : ICommand
{
    public string LongCommand => "help";
    public Option<string> ShortCommand => Option.Some("?");
    public string ShortHelp => "This is the help command, it shows a list of the available commands.";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        logger.LogTrace("Help Command executed");

        var firstArgument = arguments.FirstOrNone();

        return firstArgument.Match(
            none: () => PrintOverview(options),
            some: argument => PrintSpecificHelp(argument, container.Resolve<IEnumerable<ICommand>>()));
    }

    private bool PrintSpecificHelp(string argument, IEnumerable<ICommand> commands)
        => FindSpecificHelp(argument, commands).Match(
            none: () => PrintDeclaredHelp(argument),
            some: command =>
            {
                PrintLongHelp(command);
                return true;
            });

    /// <summary>The help of a declared command is generated from its declaration, the same as for <c>hat &lt;command&gt; --help</c>.</summary>
    private bool PrintDeclaredHelp(string argument)
        => container.Resolve<IEnumerable<IDeclaredCommand>>()
            .FirstOrNone(command => command.Name == argument)
            .Match(none: false, some: command => container.Resolve<CommandLine>().Run([command.Name, "--help"]) == ExitCode.Success);

    private Option<ICommand> FindSpecificHelp(string argument, IEnumerable<ICommand> commands)
    {
        return commands
            .Where(c => c.LongCommand == argument)
            .FirstOrNone();
    }

    private void PrintLongHelp(ICommand command)
    {
        using var resourceStream = GetHelResourceStream(command);
        using var reader = new StreamReader(resourceStream);

        WriteStreamLineByLine(reader, line => line.Replace("<PROGRAMNAME>", consoleApplicationInformation.Name));
    }

    private void WriteStreamLineByLine(TextReader reader, Func<string, string> transformLine)
    {
        while (reader.ReadLine() is { } line)
        {
            console.Writer.WriteLine(transformLine(line));
        }
    }

    private static Stream GetHelResourceStream(ICommand command)
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(GetHelpResourceName(command))
               ?? throw new NullReferenceException($"Help resource for command {command.LongCommand} not found");
    }

    private static string GetHelpResourceName(ICommand command)
    {
        return $"SortingHat.CLI.Help.{command.GetType().Name}.help";
    }

    private bool PrintOverview(IOptionParser options)
    {
        PrintHelpHeader();
        PrintHelpCommands([.. container.Resolve<IEnumerable<ICommand>>().Select(HelpRow), .. container.Resolve<IEnumerable<IDeclaredCommand>>().Select(HelpRow)]);
        PrintHelpOptions(container.Resolve<IEnumerable<IOption>>());
        PrintTagVariables(options);
        PrintHelpExamples();
        return true;
    }

    private void PrintHelpOptions(IEnumerable<IOption> lazyOptions)
    {
        var options = lazyOptions.ToList();
        if (options.Any())
        {
            console.Writer.WriteLine();
            console.Writer.WriteLine("Options");
            console.Writer.WriteLine();

            var table = HelpTable();
            foreach (var option in options)
            {
                table.Append(FormatOption(option.ShortOption, "-"), FormatOption(option.LongOption, "--"), option.ShortHelp);
            }

            table.AppendSeparator();
            table.WriteTo(console.Writer);
        }
    }

    private string FormatOption(Option<string> option, string optionPrefix)
    {
        return option.Match(
            none: string.Empty,
            some: o => $"{optionPrefix}{o}");
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

    private ConsoleTable HelpTable()
    {
        var table = new ConsoleTable(3);

        table.Columns[0].PaddingLeft = 2;

        return table;
    }

    private static (CommandGrouping Grouping, string Name, string Aliases, string Description) HelpRow(ICommand command)
        => (command.CommandGrouping, command.LongCommand, command.ShortCommand.GetOrElse(string.Empty), command.ShortHelp);

    private static (CommandGrouping Grouping, string Name, string Aliases, string Description) HelpRow(IDeclaredCommand command)
        => (command.CommandGrouping, command.Name, string.Join(", ", command.Aliases), command.Description);

    private void PrintHelpCommands(IReadOnlyList<(CommandGrouping Grouping, string Name, string Aliases, string Description)> commands)
    {
        if (commands.Any())
        {
            console.Writer.WriteLine();
            console.Writer.WriteLine("Commands");
            console.Writer.WriteLine();

            var table = HelpTable();
            foreach (var commandGrouping in Enum.GetValues(typeof(CommandGrouping)).Cast<CommandGrouping>())
            {
                foreach (var command in commands.Where(c => c.Grouping == commandGrouping))
                {
                    table.Append(command.Name, command.Aliases, command.Description);
                }

                table.AppendSeparator();
            }

            table.WriteTo(console.Writer);
        }
    }

    private void PrintHelpHeader()
    {
        console.Writer.WriteLine($"{console.Application.Name} <command> [<arguments>] [<options>]:");
    }

    private bool PrintTagVariables(IOptionParser options)
    {
        var autoTagHandler = container.Resolve<IAutoTagHandler>();
        console.Writer.WriteLine("Possible Tag Variables:");
        console.Writer.WriteLine();
        foreach (var tag in autoTagHandler.AutoTags.OrderBy(tag => tag.AutoTagKey))
        {
            console.Writer.WriteLine($"* {tag.HumanReadableAutoTagsKey}");
            if (options.HasOption<VerboseOption>())
            {
                console.Writer.WriteLine($"=>  {tag.Description}");
                console.Writer.WriteLine();
            }
        }

        return true;
    }
}
