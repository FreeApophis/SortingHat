using Autofac;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SortingHat.CLI.Commands
{
    class HelpCommand : ICommand
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

        private void PrintLongHelp(ICommand command)
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
            Console.WriteLine("Sortinghat <command> [arguments]:");
            Console.WriteLine("");
            Console.WriteLine("available commands:");
            Console.WriteLine("");
            foreach (var command in _container.Resolve<IEnumerable<ICommand>>())
            {
                Console.WriteLine($"  {command.LongCommand,-12} {command.ShortCommand,-2} {command.ShortHelp}");
            }
        }

        public string LongCommand => "help";
        public string ShortCommand => "?";

        public string ShortHelp => "This is the help command, it shows a list of the available commands.";

    }
}
