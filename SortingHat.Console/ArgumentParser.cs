using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.CLI.Commands;
using System.Collections.Generic;

namespace SortingHat.CLI
{

    class ArgumentParser
    {
        private readonly IEnumerable<string> _arguments;
        private readonly IContainer _container;
        private List<ICommand> _commands = new List<ICommand>();

        public ArgumentParser(IEnumerable<string> arguments, IContainer container)
        {
            _arguments = arguments;
            _container = container;

            RegisterCommands();

        }

        private void RegisterCommands()
        {
            _commands.Add(new HelpCommand(_container));
            _commands.Add(new InitCommand(_container));

            _commands.Add(new ListTagsCommand(_container));
            _commands.Add(new AddTagCommand(_container));

            _commands.Add(new TagFileCommand(_container));
            _commands.Add(new FindFilesCommand(_container));

            _commands.Add(new RepairCommand(_container));
            _commands.Add(new SortCommand(_container));
            _commands.Add(new IdentifyCommand(_container));
        }

        internal void Execute()
        {
            foreach (var command in _commands)
            {
                if (command.Match(_arguments))
                {
                    if (command.Execute(_arguments) == false)
                    {
                        using (var scope = _container.BeginLifetimeScope())
                        {
                            var logger = scope.Resolve<ILogger>();
                            logger.Log(LogLevel.Error, "Command Execution failed!");
                        }

                    }

                    return;
                }
            }
        }
    }
}
