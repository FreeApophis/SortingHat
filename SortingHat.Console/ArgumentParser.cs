using SortingHat.API.DI;
using SortingHat.CLI.Commands;
using System;
using System.Collections.Generic;

namespace SortingHat.CLI
{

    class ArgumentParser
    {
        private readonly IEnumerable<string> _arguments;
        private readonly IServices _services;
        private List<ICommand> _commands = new List<ICommand>();

        public ArgumentParser(IEnumerable<string> arguments, IServices services)
        {
            _arguments = arguments;
            _services = services;

            RegisterCommands();

        }

        private void RegisterCommands()
        {
            _commands.Add(new HelpCommand(_services));
            _commands.Add(new InitCommand(_services));

            _commands.Add(new ListTagsCommand(_services));
            _commands.Add(new AddTagCommand(_services));

            _commands.Add(new TagFileCommand(_services));

            _commands.Add(new RepairCommand(_services));
            _commands.Add(new SortCommand(_services));
            _commands.Add(new IdentifyCommand(_services));
        }

        internal void Execute()
        {
            foreach (var command in _commands)
            {
                if (command.Match(_arguments))
                {
                    if (command.Execute(_arguments) == false)
                    {
                        _services.Logger.Log("Command Execution failed!");
                    }

                    return;
                }
            }

            _services.Logger.Log("Unknown command, try help!");
        }
    }
}
