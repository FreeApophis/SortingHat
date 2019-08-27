using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using apophis.CLI;
using Funcky.Monads;
using SortingHat.API.AutoTag;
using SortingHat.CLI.Options;
using Console = apophis.CLI.Console;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class HelpCommand : ICommand
    {
        private readonly ILogger<HelpCommand> _logger;
        private readonly Console _console;
        private readonly IComponentContext _container;
        private readonly IConsoleApplicationInformationProvider _consoleApplicationInformation;

        public HelpCommand(ILogger<HelpCommand> logger, Console console, IComponentContext container, IConsoleApplicationInformationProvider consoleApplicationInformation)
        {
            _logger = logger;
            _console = console;
            _container = container;
            _consoleApplicationInformation = consoleApplicationInformation;
        }

        public string LongCommand => "help";
        public Option<string> ShortCommand => Option.Some("?");
        public string ShortHelp => "This is the help command, it shows a list of the available lazyCommands.";
        public CommandGrouping CommandGrouping => CommandGrouping.General;


        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            _logger.LogTrace("Help Command executed");

            if (arguments.Any())
            {
                return PrintSpecificHelp(arguments, _container.Resolve<IEnumerable<ICommand>>());
            }

            PrintOverview(options);
            return true;

        }

        private bool PrintSpecificHelp(IEnumerable<string> arguments, IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                if (command.LongCommand == arguments.First())
                {
                    PrintLongHelp(command);
                    return true;
                }
            }

            return false;
        }

        private void PrintLongHelp(ICommand command)
        {
            using var resourceStream = GetHelResourceStream(command);
            using var reader = new StreamReader(resourceStream);

            WriteStreamLineByLine(reader, line => line.Replace("<PROGRAMNAME>", _consoleApplicationInformation.Name));
        }

        private void WriteStreamLineByLine(StreamReader reader, Func<string, string> transformLine)
        {
            while (reader.ReadLine() is { } line)
            {
                _console.Writer.WriteLine(transformLine(line));
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

        private void PrintOverview(IOptionParser options)
        {
            PrintHelpHeader();
            PrintHelpCommands(_container.Resolve<IEnumerable<ICommand>>());
            PrintHelpOptions(_container.Resolve<IEnumerable<IOption>>());
            PrintTagVariables(options);
            PrintHelpExamples();
        }

        private void PrintHelpOptions(IEnumerable<IOption> lazyOptions)
        {
            var options = lazyOptions.ToList();
            if (options.Any())
            {
                _console.Writer.WriteLine();
                _console.Writer.WriteLine("Options");
                _console.Writer.WriteLine();

                var table = HelpTable();
                foreach (var option in options)
                {
                    table.Append(FormatOption(option.ShortOption, "-"), FormatOption(option.LongOption, "--"), option.ShortHelp);
                }
                table.AppendSeparator();
                table.WriteTo(_console.Writer);
            }
        }

        private string FormatOption(Option<string> option, string optionPrefix)
        {
            return option.Match(
                none: "",
                some: o => $"{optionPrefix}{o}"
                );
        }

        private void PrintHelpExamples()
        {
            _console.Writer.WriteLine();
            _console.Writer.WriteLine("Examples:");
            _console.Writer.WriteLine();
            _console.Writer.WriteLine($"  {_console.Application.Name} find (:tax:2018 or :tax:2017) and :bank");
            _console.Writer.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
            _console.Writer.WriteLine();
            _console.Writer.WriteLine($"  {_console.Application.Name} auto-tag :Files:{{FileType.Category}}:{{CameraMake}} :Taken:{{Taken.Year}} *");
            _console.Writer.WriteLine("    This will tag all files in the current directory, the possible automatic tags depend on your plugins.");
        }

        private ConsoleTable HelpTable()
        {
            var table = new ConsoleTable(3);

            table.Columns[0].PaddingLeft = 2;

            return table;
        }

        private void PrintHelpCommands(IEnumerable<ICommand> lazyCommands)
        {
            var commands = lazyCommands.ToList();
            if (commands.Any())
            {
                _console.Writer.WriteLine();
                _console.Writer.WriteLine("Commands");
                _console.Writer.WriteLine();

                var table = HelpTable();
                foreach (var commandGrouping in Enum.GetValues(typeof(CommandGrouping)).Cast<CommandGrouping>())
                {
                    foreach (var command in commands.Where(c => c.CommandGrouping == commandGrouping))
                    {
                        var shortCommand = command.ShortCommand.Match("", c => c);
                        table.Append(command.LongCommand, shortCommand, command.ShortHelp);
                    }
                    table.AppendSeparator();
                }
                table.WriteTo(_console.Writer);
            }
        }

        private void PrintHelpHeader()
        {
            _console.Writer.WriteLine($"{_console.Application.Name} <command> [<arguments>] [<options>]:");
        }

        private bool PrintTagVariables(IOptionParser options)
        {
            var autoTagHandler = _container.Resolve<IAutoTagHandler>();
            _console.Writer.WriteLine("Possible Tag Variables:");
            _console.Writer.WriteLine();
            foreach (var tag in autoTagHandler.AutoTags.OrderBy(tag => tag.AutoTagKey))
            {
                _console.Writer.WriteLine($"* {tag.HumanReadableAutoTagsKey}");
                if (options.HasOption<VerboseOption>())
                {
                    _console.Writer.WriteLine($"=>  {tag.Description}");
                    _console.Writer.WriteLine();
                }
            }

            return true;
        }
    }
}
