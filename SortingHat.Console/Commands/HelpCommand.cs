using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Funcky.Monads;
using SortingHat.CliAbstractions;
using SortingHat.CliAbstractions.Formatting;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class HelpCommand : ICommand
    {
        private readonly ILogger<HelpCommand> _logger;
        private readonly IConsoleWriter _consoleWriter;
        private readonly IComponentContext _container;

        public HelpCommand(ILogger<HelpCommand> logger, IConsoleWriter consoleWriter, IComponentContext container)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            _logger.LogTrace("Help Command executed");

            if (arguments.Any())
            {
                return FindCommand(arguments, _container.Resolve<IEnumerable<ICommand>>());
            }

            PrintOverview();
            return true;

        }

        private bool FindCommand(IEnumerable<string> arguments, IEnumerable<ICommand> commands)
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

            _consoleWriter.WriteLine(reader.ReadToEnd());
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

        private void PrintOverview()
        {
            PrintHelpHeader();
            PrintHelpCommands(_container.Resolve<IEnumerable<ICommand>>());
            PrintHelpExamples();
        }

        private void PrintHelpExamples()
        {
            _consoleWriter.WriteLine();
            _consoleWriter.WriteLine("Examples:");
            _consoleWriter.WriteLine();
            _consoleWriter.WriteLine("  hat.exe find (:tax:2018 or :tax:2017) and :bank");
            _consoleWriter.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
            _consoleWriter.WriteLine();
            _consoleWriter.WriteLine("  auto-tag :Files:{FileType.Category}:{CameraMake} :Taken:{Taken.Year} *");
            _consoleWriter.WriteLine("    This will tag all files in the current directory, the possible automatic tags depend on your plugins.");
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
                _consoleWriter.WriteLine();
                _consoleWriter.WriteLine("Commands");
                _consoleWriter.WriteLine();

                var table = HelpTable();
                foreach (var commandGrouping in Enum.GetValues(typeof(CommandGrouping)).Cast<CommandGrouping>())
                {
                    foreach (var command in commands.Where(c => c.CommandGrouping == commandGrouping))
                    {
                        var shortCommand = command.ShortCommand.Match("", c => c);
                        table.Append(command.LongCommand, shortCommand, command.ShortHelp);
                    }
                    table.AppendSeperator();
                }
                table.WriteTo(_consoleWriter);
            }
        }

        private void PrintHelpHeader()
        {
            _consoleWriter.WriteLine("Sortinghat <command> [arguments]:");
        }

        public string LongCommand => "help";
        public Option<string> ShortCommand => Option.Some("?");

        public string ShortHelp => "This is the help command, it shows a list of the available lazyCommands.";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
