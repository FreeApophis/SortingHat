using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    class RepairCommand : ICommand
    {
        private const string Command = "repair";
        private readonly IContainer _container;

        public RepairCommand(IContainer container)
        {
            _container = container;
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
