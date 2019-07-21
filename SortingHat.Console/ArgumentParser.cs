using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.DI;
using Funcky.Extensions;

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

                // Register short command, if there is one
                command.ShortCommand.AndThen(shortCommand => _commandTargets[shortCommand] = command);
            }
        }

        internal void Execute(IEnumerable<string> lazyArguments)
        {
            var arguments = lazyArguments.ToList();
            if (arguments.Any())
            {
                _commandTargets
                    .TryGetValue(key: arguments.First())
                    .Match(
                        none: () => UnknownCommandError(arguments.First()),
                        some: c => ExecuteCommand(c, arguments));

            } else
            {
                Console.WriteLine("Maybe run 'hat help'");
            }
        }

        private void ExecuteCommand(ICommand command, List<string> arguments)
        {
            if (command.Execute(TagAndFileArguments(arguments), new Options(OptionArguments(arguments))) == false)
            {
                _logger.Value.LogWarning("Command Execution failed!");
            }
        }

        private void UnknownCommandError(string commandName)
        {
            _logger.Value.LogWarning($"Unknown command: '{commandName}' cannot be executed.");
            Console.WriteLine($"Unknown command: '{commandName}'");
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
