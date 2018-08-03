using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    class SortCommand : ICommand
    {
        private const string Command = "sort";
        private readonly IServices _services;

        public SortCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
