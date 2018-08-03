using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    class RepairCommand : ICommand
    {
        private const string Command = "repair";
        private IServices _services;

        public RepairCommand(IServices services)
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
