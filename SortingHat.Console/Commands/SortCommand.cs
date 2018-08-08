using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    class SortCommand : ICommand
    {
        private const string Command = "sort";
        private readonly IContainer _container;

        public SortCommand(IContainer container)
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
