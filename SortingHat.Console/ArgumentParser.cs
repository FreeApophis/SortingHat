using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.DI;

namespace SortingHat.CLI
{
    [UsedImplicitly]
    internal class ArgumentParser
    {
        private readonly Dictionary<string, ICommand> _commandTargets = new Dictionary<string, ICommand>();
        private readonly Lazy<ILogger<ArgumentParser>> _logger;

        public ArgumentParser(Lazy<ILogger<ArgumentParser>> logger, IEnumerable<ICommand> commands)
        {
            _logger = logger;

            RegisterCommands(commands);
        }

        private void RegisterCommands(IEnumerable<ICommand> commands)
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
            if (arguments.Any())
            {
                if (_commandTargets.TryGetValue(arguments.First(), out var command))
                {

                    if (command.Execute(TagAndFileArguments(arguments), new Options(OptionArguments(arguments))) == false)
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
            else
            {
                Console.WriteLine("Maybe run 'hat help'");
            }

        }

        private static IEnumerable<string> TagAndFileArguments(IEnumerable<string> arguments)
        {
            return arguments.Skip(1).Where(s => s.StartsWith("-") == false);
        }

        private static IEnumerable<string> OptionArguments(IEnumerable<string> arguments)
        {
            return arguments.Skip(1).Where(s => s.StartsWith("-"));
        }
    }
}
