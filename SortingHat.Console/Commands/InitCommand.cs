using Autofac;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class InitCommand : ICommand
    {
        private const string Command = "init";
        private IContainer _container;

        public InitCommand(IContainer container)
        {
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                using (var scope = _container.BeginLifetimeScope())
                {
                    var db = scope.Resolve<IDatabase>();
                    db.Setup();
                }
            }

            return false;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
