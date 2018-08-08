using Microsoft.Extensions.Logging;
using SortingHat.CLI.Commands;
using System;
using System.Collections.Generic;

namespace SortingHat.CLI
{

    class ArgumentParser
    {
        private IEnumerable<ICommand> _commands;
        Lazy<ILogger<ArgumentParser>> _logger;

        public ArgumentParser(Lazy<ILogger<ArgumentParser>> logger, IEnumerable<ICommand> commands)
        {
            _commands = commands;
            _logger = logger;
        }

        internal void Execute(IEnumerable<string> arguments)
        {
            foreach (var command in _commands)
            {
                if (command.Match(arguments))
                {
                    if (command.Execute(arguments) == false)
                    {
                        _logger.Value.Log(LogLevel.Warning, "Command Execution failed!");
                    }

                    break;
                }
            }
        }
    }
}
