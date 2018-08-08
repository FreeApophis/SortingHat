using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class HelpCommand : ICommand
    {
        private const string Command = "help";
        private readonly IContainer _container;

        public HelpCommand(IContainer container)
        {
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("XXX");
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
