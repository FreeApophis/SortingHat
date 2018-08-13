using Microsoft.Extensions.Logging;
using SortingHat.CLI.Commands;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI
{

    class ArgumentParser
    {
        private Dictionary<string, ICommand> _commandTargets = new Dictionary<string, ICommand>();
        Lazy<ILogger<ArgumentParser>> _logger;

        public ArgumentParser(Lazy<ILogger<ArgumentParser>> logger, IEnumerable<ICommand> commands)
        {
            AssignCommands(commands);
            _logger = logger;
        }

        private void AssignCommands(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                _commandTargets[command.LongCommand] = command;
                if (command.ShortCommand != null)
                {
                    _commandTargets[command.ShortCommand] = command;
                }
            }
        }

        internal void Execute(IEnumerable<string> arguments)
        {
            if (_commandTargets.TryGetValue(arguments.First(), out var command))
            {
                if (command.Execute(arguments.Skip(1)) == false)
                {
                    _logger.Value.LogWarning("Command Execution failed!");
                }
            }
            else
            {
                _logger.Value.LogWarning($"Unknown command: '{arguments.First()}' cannot be executed.");
                Console.WriteLine($"Unknown command: '{arguments.First()}'");
            }

        }
    }
}
