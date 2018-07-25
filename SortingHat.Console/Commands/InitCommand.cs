using SortingHat.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class InitCommand : ICommand
    {
        private const string Command = "init";
        private IServices _services;

        public InitCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                return true;
            }

            if (arguments.Count() == 2)
            {
                return true;
            }

            return false;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
