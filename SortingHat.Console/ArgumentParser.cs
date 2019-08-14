using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.DI;
using Funcky.Extensions;
using Console = apophis.CLI.Console;

namespace SortingHat.CLI
{
    [UsedImplicitly]
    internal class ArgumentParser
    {
        private readonly Dictionary<string, ICommand> _commandTargets = new Dictionary<string, ICommand>();
        private readonly Lazy<ILogger<ArgumentParser>> _logger;
        private readonly Console _console;

        public ArgumentParser(Lazy<ILogger<ArgumentParser>> logger, Console console, IEnumerable<ICommand> commands)
        {
            _logger = logger;
            _console = console;

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

        internal bool Execute(IEnumerable<string> lazyArguments)
        {
            var arguments = lazyArguments.ToList();
            if (arguments.Any())
            {
                return _commandTargets
                    .TryGetValue(key: arguments.First())
                    .Match(
                        none: () => UnknownCommandError(arguments.First()),
                        some: c => ExecuteCommand(c, arguments));

            }

            _console.Writer.WriteLine($"Maybe run '{_console.Application.Name} help'");
            return false;
        }

        private bool ExecuteCommand(ICommand command, List<string> arguments)
        {
            var success = command.Execute(TagAndFileArguments(arguments), new OptionParser(OptionArguments(arguments)));

            if (success == false)
            {
                _logger.Value.LogWarning("Command Execution failed!");
            }

            return success;
        }

        private bool UnknownCommandError(string commandName)
        {
            _logger.Value.LogWarning($"Unknown command: '{commandName}' cannot be executed.");
            _console.Writer.WriteLine($"Unknown command: '{commandName}'");

            return false;
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
