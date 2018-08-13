using Autofac;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class HelpCommand : ICommand
    {
        private const string Command = "help";
        private readonly ILogger<HelpCommand> _logger;
        private readonly IComponentContext _container;

        public HelpCommand(ILogger<HelpCommand> logger, IComponentContext container)
        {
            _logger = logger;
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
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
            else
            {
                PrintOverview();
                return true;
            }

        }

        private void PrintLongHelp(ICommand command)
        {
            Console.WriteLine(command.ShortHelp);
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
        public string ShortCommand => null;

        public string ShortHelp => "This is the help command, it shows a list of the available commands.";

    }
}
