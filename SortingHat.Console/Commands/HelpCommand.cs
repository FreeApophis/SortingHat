using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            _logger.Log(LogLevel.Information, "Execute Help Command");


            foreach (var command in _container.Resolve<IEnumerable<ICommand>>())
            {
                Console.WriteLine($"Command {command.GetType().Name} offers {command.ShortHelp}");
            }

            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }

        public string ShortHelp => "This is the help command, it shows a list of the available commands.";


    }
}
