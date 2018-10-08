using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class HelpCommand : ICommand
    {
        private readonly ILogger<HelpCommand> _logger;
        private readonly IComponentContext _container;

        public HelpCommand(ILogger<HelpCommand> logger, IComponentContext container)
        {
            _logger = logger;
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

        private static void PrintLongHelp(ICommand command)
        {
            using (var resourceStream = GetHelResourceStream(command))
            using (var reader = new StreamReader(resourceStream))
            {
                Console.WriteLine(reader.ReadToEnd());
            }

        }

        private static Stream GetHelResourceStream(ICommand command)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(GetHelpResourceName(command));
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
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("  hat.exe find (:tax:2018 or :tax:2017) and :bank");
            Console.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
            Console.WriteLine();
            Console.WriteLine("  auto-tag :Files:{FileType.Category}:{CameraMake} :Taken:{Taken.Year} *");
            Console.WriteLine("    This will tag all files in the current directory, the possible automatic tags depend on your plugins.");
        }

        private ConsoleTable HelpTable()
        {
            var table = new ConsoleTable(3);

            table.Columns[0].PaddingLeft = 2;

            return table;
        }

        private void PrintHelpCommands(IEnumerable<ICommand> commands)
        {
            if (commands.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Commands");
                Console.WriteLine();

                var table = HelpTable();
                foreach (CommandGrouping commandGrouping in Enum.GetValues(typeof(CommandGrouping)))
                {
                    foreach (var command in commands.Where(c => c.CommandGrouping == commandGrouping))
                    {
                        table.Append(command.LongCommand, command.ShortCommand, command.ShortHelp);
                    }
                    table.AppendSeperator();
                }
                Console.WriteLine(table.ToString());
            }
        }

        private static void PrintHelpHeader()
        {
            Console.WriteLine("Sortinghat <command> [arguments]:");
        }

        public string LongCommand => "help";
        public string ShortCommand => "?";

        public string ShortHelp => "This is the help command, it shows a list of the available commands.";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
