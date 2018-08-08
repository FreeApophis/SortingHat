using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.CLI.Commands;
using System.Collections.Generic;

namespace SortingHat.CLI
{

    class ArgumentParser
    {
        private IEnumerable<ICommand> _commands;
        ILogger<ArgumentParser> _logger;

        public ArgumentParser(ILogger<ArgumentParser> logger , IEnumerable<ICommand> commands)
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
                        _logger.Log(LogLevel.Error, "Command Execution failed!");
                    }

                    return;
                }
            }
        }
    }
}
