using Autofac;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using SortingHat.CLI.Output;

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

        public bool Execute(IEnumerable<string> arguments)
        {
            _logger.LogTrace("Help Command executed");

            if (arguments.Any())
            {
                foreach (var command in _container.Resolve<IEnumerable<ICommand>>())
                {
                    if (command.LongCommand == arguments.First())
                    {
                        PrintLongHelp(command);
                        return true;
                    }
                }
                return false;
            }

            PrintOverview();
            return true;

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
            PrintHelpCommands();
            PrintHelpExamples();
        }

        private void PrintHelpExamples()
        {
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("");
            Console.WriteLine("  hat.exe find (:tax:2018 or :tax:2017) and :bank");
            Console.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
        }

        private ConsoleTable HelpTable()
        {
            var table = new ConsoleTable();

            table.Columns.Add(new ConsoleTableColumn());
            table.Columns.Add(new ConsoleTableColumn());
            table.Columns.Add(new ConsoleTableColumn());

            return table;
        }

        private void PrintHelpCommands()
        {
            var commands = _container.Resolve<IEnumerable<ICommand>>();

            var table = HelpTable();
            foreach (var command in _container.Resolve<IEnumerable<ICommand>>())
            {
                Console.WriteLine($"  {command.LongCommand,-12} {command.ShortCommand,-4} {command.ShortHelp}");
                //table.Append(command.LongCommand, command.ShortCommand, command.ShortHelp);
            }
            //Console.WriteLine(table.ToString());
        }

        private static void PrintHelpHeader()
        {
            Console.WriteLine("Sortinghat <command> [arguments]:");
            Console.WriteLine("");
            Console.WriteLine("available commands:");
            Console.WriteLine("");
        }

        public string LongCommand => "help";
        public string ShortCommand => "?";

        public string ShortHelp => "This is the help command, it shows a list of the available commands.";

    }
}
