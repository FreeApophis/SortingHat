using System;
using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ImportProjectCommand : ICommand
    {
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "import";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Imports a project file into the db.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            throw new NotImplementedException();
        }
    }
}