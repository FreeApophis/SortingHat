using Autofac;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using SortingHat.CLI.Output;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class HelpCommand : ICommand
    {
        private readonly ILogger<HelpCommand> _logger;
        private readonly IPluginLoader _pluginLoader;
        private readonly IComponentContext _container;

        public HelpCommand(ILogger<HelpCommand> logger, IPluginLoader pluginLoader, IComponentContext container)
        {
            _logger = logger;
            _pluginLoader = pluginLoader;
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            _logger.LogTrace("Help Command executed");

            if (arguments.Any())
            {
                return FindCommand(arguments, _container.Resolve<IEnumerable<ICommand>>()) &&
                       FindCommand(arguments, _pluginLoader.Commands);
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
            PrintHelpCommands(_container.Resolve<IEnumerable<ICommand>>(), "Available commands");
            PrintHelpCommands(_pluginLoader.Commands, "Plugin commands:");
            PrintHelpExamples();
        }

        private void PrintHelpExamples()
        {
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("  hat.exe find (:tax:2018 or :tax:2017) and :bank");
            Console.WriteLine("    This will output all files which are tagged as bank documents for tax in 2017 or 2018.");
        }

        private ConsoleTable HelpTable()
        {
            var table = new ConsoleTable(3);

            table.Columns[0].PaddingLeft = 2;

            return table;
        }

        private void PrintHelpCommands(IEnumerable<ICommand> commands, string title)
        {
            if (commands.Any())
            {
                Console.WriteLine();
                Console.WriteLine(title);
                Console.WriteLine();

                var table = HelpTable();
                foreach (var command in commands)
                {
                    table.Append(command.LongCommand, command.ShortCommand, command.ShortHelp);
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

    }
}
